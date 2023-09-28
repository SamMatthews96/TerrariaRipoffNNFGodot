using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static Godot.GD;

namespace TerrariaRipoffNNF.scripts;

public class BlockStability {
    public bool IsSupportBlock { get; private set; }

    public float ExcessWeight {
        get {
            foreach (float burdenValue in outboundBurden.Values) {
                Print(burdenValue);
            }


            Print(block);
            Print(block.BlockResource.Weight);


            return outboundBurden.Values.Aggregate(block.BlockResource.Weight,
                (current, outboundBurdenValue) => current - outboundBurdenValue);
        }
    }

    public bool IsStable => IsSupportBlock || ExcessWeight == 0;

    private Block block;
    private UnstableBlockGroup unstableBlockGroup;

    private readonly Dictionary<Direction, float> outboundBurden = new() {
        { Direction.Down, 0f },
        { Direction.Up, 0f },
        { Direction.Left, 0f },
        { Direction.Right, 0f }
    };

    public BlockStability(Block block) {
        this.block = block;
        Block blockBeneath = block.GetBlockInDirection(Direction.Down);
        IsSupportBlock = block.YPosition == 0 ||
                         (blockBeneath is not null && blockBeneath.Stability.IsSupportBlock);
        if (IsSupportBlock) {
            SetAboveBlocksIsSupport(true);
        }

        block.OnCreated += Block_OnCreated;
        block.OnDestroyed += Block_OnDestroyed;
        block.OnNeighbourDestroyed += Block_OnNeighbourDestroyed;
    }

    private void Block_OnCreated(object sender, EventArgs e) {
        ResolveStability();
    }

    private void Block_OnDestroyed(object sender, EventArgs e) {
        if (unstableBlockGroup is not null) {
            List<Block> weightedBlocks = unstableBlockGroup.GetBlocksWithExcessWeight();
            unstableBlockGroup?.Destroy();
            if (weightedBlocks.Count > 0) {
                weightedBlocks[0].Stability.CreateUnstableGroup();
            }
        }

        block = null;
    }

    private void Block_OnNeighbourDestroyed(
        object sender, Block.OnNeighbourDestroyedEventArgs e) {
        outboundBurden[e.Direction] = 0f;

        if (e.Direction == Direction.Down) {
            IsSupportBlock = false;
            ResolveStability();
            SetAboveBlocksIsSupport(false);
        } else {
            ResolveStability();
        }
    }

    private void SetAboveBlocksIsSupport(bool value) {
        Block aboveBlock = block.GetBlockInDirection(Direction.Up);
        while (aboveBlock is not null) {
            aboveBlock.Stability.IsSupportBlock = value;
            aboveBlock.Stability.ResolveStability();
            aboveBlock = aboveBlock.GetBlockInDirection(Direction.Up);
        }
    }

    private void ResolveStability() {
        if (IsSupportBlock) return;
        ResolveExcessWeight();

        if (ExcessWeight > 0) {
            //this block cannot be supported
            foreach (Block adjacentBlock in block.GetAdjacentBlocks().Values) {
                adjacentBlock?.Stability.unstableBlockGroup?.Destroy();
            }

            CreateUnstableGroup();
        } else {
            //this block is stable

            List<Block> excessWeightedBlocks = new();
            foreach (var adjacentBlock in block.GetAdjacentBlocks().Values.Where(
                         adjacentBlock => adjacentBlock?.Stability.unstableBlockGroup is not null)) {
                excessWeightedBlocks.AddRange(
                    adjacentBlock.Stability.unstableBlockGroup.GetBlocksWithExcessWeight());
                adjacentBlock.Stability.unstableBlockGroup.Destroy();
            }

            foreach (Block excessWeightedBlock in excessWeightedBlocks.Where(
                         excessWeightedBlock => excessWeightedBlock.Stability.unstableBlockGroup is null)) {
                excessWeightedBlock.Stability.ResolveExcessWeight();
                if (excessWeightedBlock.Stability.ExcessWeight > 0) {
                    excessWeightedBlock.Stability.CreateUnstableGroup();
                }
            }
        }
    }

    private void ResolveExcessWeight() {
        while (ExcessWeight > 0) {
            List<Block> currentAugmentingPath = GetAugmentingPath();
            if (currentAugmentingPath is null) return;

            float pathStrength = GetAugmentingPathStrength(currentAugmentingPath);
            float strengthDelta = ExcessWeight > pathStrength ? pathStrength : ExcessWeight;
            IncreaseSupportPath(currentAugmentingPath, strengthDelta);
        }
    }


    //consider refining the representation of an augmenting path
    // currently it is a series of blocks, where the relative position is 
    //recalculated which iterating over it. Instead of saving the support
    //path as a list, could it be saved as a list of Direction,Block pairs?

    //reconsider the ways augmenting paths are calculated
    //this way could potentially be very inefficient in larger structures, and have many
    //redundant calculations. 
    /*
        from current block list
        create a block list for each valid path that leads from it
        until stable is found, or out of options
     */
    private List<Block> GetAugmentingPath() {
        var currentPath = new List<Block> { block };
        var invalidBlocks = new List<Block>();

        while (!currentPath[^1].Stability.IsSupportBlock) {
            Dictionary<Direction, Block> adjacentBlocks = currentPath[^1].GetAdjacentBlocks();
            Block nextBlock = null;
            foreach (var (direction, adjacentBlock) in adjacentBlocks) {
                if (adjacentBlock is null) continue;
                if (invalidBlocks.Contains(adjacentBlock)) continue;
                if (currentPath.Contains(adjacentBlock)) continue;

                if (currentPath[^1].Stability.outboundBurden[direction] >=
                    adjacentBlock.BlockResource.TensileStrength) continue;
                nextBlock = adjacentBlock;
                break;
            }

            if (nextBlock is null) {
                if (currentPath[^1] == block) return null;
                // it may not be the block this is invalid, but the connection of the two
                // in which case, blocks would be incorrectly considered as invalid
                invalidBlocks.Add(currentPath[^1]);
                currentPath.RemoveAt(currentPath.Count - 1);
            } else {
                currentPath.Add(nextBlock);
            }
        }

        return currentPath;
    }

    private static float GetAugmentingPathStrength(List<Block> augmentingPath) {
        float minStrength = int.MaxValue;

        for (int i = 0; i < augmentingPath.Count - 1; i++) {
            var supported = augmentingPath[i];
            var supporting = augmentingPath[i + 1];
            Direction direction = supported.GetDirectionOfBlock(supporting);
            float existingSupportStrength = supported.Stability.outboundBurden[direction];
            minStrength = Math.Min(
                minStrength, supporting.BlockResource.TensileStrength - existingSupportStrength);
        }

        return minStrength;
    }

    private static void IncreaseSupportPath(List<Block> blockPath, float strength) {
        for (int i = 0; i < blockPath.Count - 1; i++) {
            var supportedBlock = blockPath[i];
            var supportingBlock = blockPath[i + 1];
            Direction forwardDirection = supportingBlock.GetDirectionOfBlock(supportedBlock);
            Direction backwardDirection = DirectionMethods.Opposite(forwardDirection);
            supportingBlock.Stability.outboundBurden[forwardDirection] -= strength;
            supportedBlock.Stability.outboundBurden[backwardDirection] += strength;
        }
    }

    public class UnstableBlockGroup {
        public readonly List<Block> UnstableBlocks;
        public readonly List<Block> BoundaryBlocks;
        public readonly List<Block> SupportingBlocks;

        public UnstableBlockGroup(
            List<Block> unstableBlocks, List<Block> boundaryBlocks, List<Block> supportingBlocks
        ) {
            UnstableBlocks = unstableBlocks;
            BoundaryBlocks = boundaryBlocks;
            SupportingBlocks = supportingBlocks;

            foreach (Block unstableBlock in unstableBlocks) {
                unstableBlock.Stability.unstableBlockGroup = this;
            }
        }

        public List<Block> GetBlocksWithExcessWeight() {
            return UnstableBlocks.Where(block => block.Stability.ExcessWeight > 0).ToList();
        }

        public void Destroy() {
            foreach (Block unstableBlock in UnstableBlocks) {
                unstableBlock.Stability.unstableBlockGroup = null;
            }
        }
    }

    public UnstableBlockGroup CreateUnstableGroup() {
        List<Block> unstableBlocks = new() { block };
        List<Block> boundaryBlocks = new() { block };
        List<Block> supportingBlocks = new();

        while (boundaryBlocks.Count > 0) {
            List<Block> nextBoundaryBlocks = new();
            foreach (Block unstableBlock in boundaryBlocks) {
                foreach (var (direction, adjacentBlock) in unstableBlock.GetAdjacentBlocks()) {
                    if (adjacentBlock is null) continue;
                    if (unstableBlocks.Contains(adjacentBlock)) continue;
                    if (nextBoundaryBlocks.Contains(adjacentBlock)) continue;

                    Direction forward = DirectionMethods.Opposite(direction);
                    if (Math.Abs(adjacentBlock.Stability.outboundBurden[forward]
                                 + adjacentBlock.BlockResource.TensileStrength) < 0.001f) {
                        supportingBlocks.Add(adjacentBlock);
                    } else {
                        nextBoundaryBlocks.Add(adjacentBlock);
                    }
                }
            }

            unstableBlocks.AddRange(nextBoundaryBlocks);
            boundaryBlocks = nextBoundaryBlocks;
        }

        return new UnstableBlockGroup(unstableBlocks, boundaryBlocks, supportingBlocks);
    }
}
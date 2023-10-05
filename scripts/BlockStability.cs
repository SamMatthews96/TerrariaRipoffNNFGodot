using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static Godot.GD;

namespace TerrariaRipoffNNF.scripts;

public class BlockStability {
    public bool IsSupportBlock { get; private set; }

    public float ExcessWeight => outboundBurden.Values.Aggregate(block.BlockResource.Weight,
        (current, outboundBurdenValue) => current - outboundBurdenValue);

    public bool IsStable { get; private set; } = true;

    private Block block;

    private readonly Dictionary<Direction, float> outboundBurden = new() {
        { Direction.Down, 0f },
        { Direction.Up, 0f },
        { Direction.Left, 0f },
        { Direction.Right, 0f }
    };

    public BlockStability(Block block) {
        this.block = block;
        block.OnCreated += Block_OnCreated;
        block.OnNeighbourDestroyed += Block_OnNeighbourDestroyed;
    }

    private void Block_OnCreated(object sender, EventArgs e) {
        Block blockBeneath = block.GetBlockInDirection(Direction.Down);
        IsSupportBlock = block.YPosition == 0 ||
                         (blockBeneath is not null && blockBeneath.Stability.IsSupportBlock);
        if (IsSupportBlock) {
            SetAboveBlocksIsSupport(true);
        }

        ResolveStability();
    }

    private void Block_OnNeighbourDestroyed(
        object sender, Block.OnNeighbourDestroyedEventArgs e) {
        outboundBurden[e.Direction] = 0f;

        if (e.Direction == Direction.Down) {
            IsSupportBlock = false;
        }

        ResolveStability();
        if (e.Direction == Direction.Down) {
            SetAboveBlocksIsSupport(false);
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
        if (IsSupportBlock) {
            IsStable = true;
            return;
        }

        ResolveExcessWeight();
        if (ExcessWeight == 0) {
            var adjacentBlocks = block.GetAdjacentBlocks().Values;
            Print($"{block.XPosition},{block.YPosition}");
            foreach (Block adjacentBlock in adjacentBlocks) {
                if (adjacentBlock is null) continue;
                if (adjacentBlock.Stability.IsStable) continue;

                Print($"{adjacentBlock.XPosition},{adjacentBlock.YPosition}");

                (List<Block> unstableBlocks, List<Block> supportingBlocks) =
                    adjacentBlock.Stability.GetInstabilityInformation();


                foreach (Block unstableBlock in unstableBlocks) {
                    Print("here");
                    unstableBlock.Stability.ResolveExcessWeight();
                    Print(unstableBlock.Stability.ExcessWeight);
                    // if (unstableBlock.Stability.ExcessWeight > 0) break;
                }

                bool isGroupStable =
                    unstableBlocks.TrueForAll(unstableBlock => unstableBlock.Stability.ExcessWeight == 0);

                foreach (Block unstableBlock in unstableBlocks) {
                    unstableBlock.Stability.IsStable = isGroupStable;
                }
            }
        } else {
            (List<Block> unstableBlocks, List<Block> supportingBlocks) = GetInstabilityInformation();
            bool isGroupStable =
                unstableBlocks.TrueForAll(unstableBlock => unstableBlock.Stability.ExcessWeight == 0);

            foreach (Block unstableBlock in unstableBlocks) {
                unstableBlock.Stability.IsStable = isGroupStable;
            }
        }
    }

    private void ResolveExcessWeight() {
        while (ExcessWeight > 0) {
            List<Block> currentAugmentingPath = GetAugmentingPath();
            Print("path");
            Print(currentAugmentingPath);
            if (currentAugmentingPath is null) break;

            float pathStrength = GetAugmentingPathStrength(currentAugmentingPath);
            float strengthDelta = ExcessWeight > pathStrength ? pathStrength : ExcessWeight;
            IncreaseSupportPath(currentAugmentingPath, strengthDelta);
        }
    }
    
    /*
        from current block list
        create a block list for each valid path that leads from it
        until stable is found, or out of options
     */
    private List<Block> GetAugmentingPath() {
        var currentPath = new List<Block> { block };
        var invalidBlocks = new List<Block>();
        Print($"{block.XPosition},{block.YPosition}");

        while (!currentPath[^1].Stability.IsSupportBlock) {
            Dictionary<Direction, Block> adjacentBlocks = currentPath[^1].GetAdjacentBlocks();
            Block nextBlock = null;
            Print("current");
            Print($"{currentPath[^1].XPosition},{currentPath[^1].YPosition}");
            foreach (var (direction, adjacentBlock) in adjacentBlocks) {
                Print(direction);
                Print(adjacentBlock);
            }

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
                // Print("remove");
                // Print($"{currentPath[^1].XPosition},{currentPath[^1].YPosition}");
            } else {
                // Print("add");
                // Print($"{nextBlock.XPosition},{nextBlock.YPosition}");

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

    /* if a block's weight cannot be resolved, it needs to set all neighbouring blocks to unstable
     */
    private (List<Block> unstableBlocks, List<Block> supportingBlocks) GetInstabilityInformation() {
        List<Block> unstableBlocks = new List<Block> { block };
        List<Block> boundaryBlocks = new List<Block> { block };
        List<Block> supportingBlocks = new();

        while (boundaryBlocks.Count != 0) {
            List<Block> neighbouringBlocks = new();
            foreach (Block boundaryBlock in boundaryBlocks) {
                foreach (Block adjacentBlock in boundaryBlock.GetAdjacentBlocks().Values) {
                    if (adjacentBlock is null) continue;
                    if (unstableBlocks.Contains(adjacentBlock)) continue;
                    if (neighbouringBlocks.Contains(adjacentBlock)) continue;

                    if (adjacentBlock.Stability.IsStable) {
                        if (!supportingBlocks.Contains(adjacentBlock)) {
                            supportingBlocks.Add(adjacentBlock);
                        }
                    } else {
                        neighbouringBlocks.Add(adjacentBlock);
                    }
                }
            }

            unstableBlocks.AddRange(neighbouringBlocks);
            boundaryBlocks = neighbouringBlocks;
        }

        return (unstableBlocks, supportingBlocks);
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;
using static Godot.GD;

namespace TerrariaRipoffNNF.scripts;

public class BlockStability {
    private readonly Block block;

    private readonly Dictionary<Direction, float> outboundBurden = new() {
        { Direction.Down, 0f },
        { Direction.Up, 0f },
        { Direction.Left, 0f },
        { Direction.Right, 0f }
    };

    public bool IsSupportBlock { get; private set; }
    public bool IsStable { get; private set; } = true;

    public float ExcessWeight => outboundBurden.Values.Aggregate(
        block.BlockResource.Weight, (acc, currentBurdenValue) => acc - currentBurdenValue);

    public float ExcessBurden { get; private set; } = 0;

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
            // this block is stable
            foreach (Block adjacentBlock in block.GetAdjacentBlocks().Values) {
                if (adjacentBlock is null) continue;
                if (adjacentBlock.Stability.IsStable) continue;

                List<Block> unstableBlocks = adjacentBlock.Stability.GetLocalUnstableBlocks();

                foreach (Block unstableBlock in unstableBlocks) {
                    unstableBlock.Stability.ResolveExcessWeight();
                }

                bool isGroupStable =
                    unstableBlocks.TrueForAll(unstableBlock => unstableBlock.Stability.ExcessWeight == 0);

                foreach (Block unstableBlock in unstableBlocks) {
                    unstableBlock.Stability.IsStable = isGroupStable;
                }

                if (!isGroupStable) {
                    CalculateExcessBurdens(unstableBlocks);
                }
            }
        } else {
            // this block is unstable

            List<Block> unstableBlocks = GetLocalUnstableBlocks();

            foreach (Block unstableBlock in unstableBlocks) {
                unstableBlock.Stability.IsStable = false;
            }
            
            CalculateExcessBurdens(unstableBlocks);
        }
    }

    private void ResolveExcessWeight() {
        while (ExcessWeight > 0) {
            List<Block> currentAugmentingPath = GetAugmentingPath();
            if (currentAugmentingPath is null) break;

            float pathStrength = GetAugmentingPathStrength(currentAugmentingPath);
            float strengthDelta = ExcessWeight > pathStrength ? pathStrength : ExcessWeight;
            IncreaseSupportPath(currentAugmentingPath, strengthDelta);
        }
    }

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

    private List<Block> GetLocalUnstableBlocks() {
        List<Block> unstableBlocks = new List<Block> { block };
        List<Block> boundaryBlocks = new List<Block> { block };

        while (boundaryBlocks.Count != 0) {
            List<Block> neighbouringBlocks = new();
            foreach (Block boundaryBlock in boundaryBlocks) {
                foreach (Block adjacentBlock in boundaryBlock.GetAdjacentBlocks().Values) {
                    if (adjacentBlock is null) continue;
                    if (unstableBlocks.Contains(adjacentBlock)) continue;
                    if (neighbouringBlocks.Contains(adjacentBlock)) continue;
                    if (adjacentBlock.Stability.IsSupportBlock) continue;

                    neighbouringBlocks.Add(adjacentBlock);
                }
            }

            unstableBlocks.AddRange(neighbouringBlocks);
            boundaryBlocks = neighbouringBlocks;
        }

        return unstableBlocks;
    }

    private static List<(Direction, Block)> GetUnstableSupportConnections(List<Block> unstableBlocks) {
        List<(Direction, Block)> supportConnections = new();
        foreach (Block unstableBlock in unstableBlocks) {
            foreach (var (direction, adjacentBlock) in unstableBlock.GetAdjacentBlocks()) {
                if (adjacentBlock is null) continue;
                if (!adjacentBlock.Stability.IsSupportBlock) continue;

                supportConnections.Add((direction, adjacentBlock));
            }
        }

        return supportConnections;
    }

    private static void CalculateExcessBurdens(List<Block> unstableBlocks) {
        List<(Direction direction, Block block)> supportConnections = GetUnstableSupportConnections(unstableBlocks);
        float totalWeight = unstableBlocks.Aggregate(
            0f, (acc, unstableBlock) => acc + unstableBlock.BlockResource.Weight);

        float totalSupportStrength = supportConnections.Aggregate(
            0f, (acc, supportConnection) => supportConnection.block.BlockResource.TensileStrength);

        float relativeExcess = totalWeight / totalSupportStrength;
        foreach (var (direction,supportBlock) in supportConnections) {
            Block supportedBlock = supportBlock.GetBlockInDirection(DirectionMethods.Opposite(direction));
            supportedBlock.Stability.ExcessBurden = relativeExcess;
        }
    }
}
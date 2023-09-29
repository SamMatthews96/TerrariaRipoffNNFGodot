using System;
using System.Collections.Generic;
using Godot;

using static Godot.GD;

namespace TerrariaRipoffNNF.scripts;

public class Block : IDamageable {
    #region Static

    public const int WORLD_WIDTH = 100;
    public const int WORLD_HEIGHT = 100;

    private static readonly Block[,] WorldBlocks = new Block[WORLD_WIDTH, WORLD_HEIGHT];


    public static Block CreateBlock(int xPosition, int yPosition, BlockResource blockSo) {
        return WorldBlocks[xPosition, yPosition] = new Block(xPosition, yPosition, blockSo);
    }

    public static Block GetBlockAtPosition(int xPosition, int yPosition) {
        return WorldBlocks[xPosition, yPosition];
    }

    public static List<Block> GetBlocksInArea(
        int xStartPosition, int yStartPosition, int xEndPosition, int yEndPosition) {
        
        List<Block> blockList = new();
        for (int x = xStartPosition; x < xEndPosition; x++) {
            for (int y = yStartPosition; y < yEndPosition; y++) {
                var block = GetBlockAtPosition(x, y);
                if (block is not null) {
                    blockList.Add(block);
                }
            }
        }
        
        return blockList;
    }

    #endregion


    public event EventHandler OnCreated;
    public event EventHandler OnDestroyed;
    public event EventHandler<OnNeighbourDestroyedEventArgs> OnNeighbourDestroyed;

    public class OnNeighbourDestroyedEventArgs : EventArgs {
        public Direction Direction;
    }

    public event EventHandler<IDamageable.OnHitEventArgs> OnHit;

    public readonly int XPosition;
    public readonly int YPosition;

    public readonly BlockResource BlockResource;

    public readonly BlockStability Stability;
    public readonly Health Health;
    public StaticBody2D BlockObject { get; private set; }

    private Block(int xPosition, int yPosition, BlockResource blockResource) {
        if (GetBlockAtPosition(xPosition, yPosition) is not null) {
            throw new Exception($"there is already a block at {xPosition},{yPosition}");
        }

        XPosition = xPosition;
        YPosition = yPosition;
        BlockResource = blockResource;

        Stability = new BlockStability(this);

        Health = new Health(this, blockResource.MaxHealth);
        Health.OnHealthReachingZero += Health_OnHealthReachingZero;

        OnCreated?.Invoke(this, EventArgs.Empty);
    }

    private void Health_OnHealthReachingZero(object sender, EventArgs e) {
        Destroy();
    }

    public void Destroy() {
        OnDestroyed?.Invoke(this, EventArgs.Empty);

        foreach (var (direction, adjacentBlock) in GetAdjacentBlocks()) {
            adjacentBlock?.OnNeighbourDestroyed(this, new OnNeighbourDestroyedEventArgs {
                Direction = DirectionMethods.Opposite(direction)
            });
        }
        
        WorldBlocks[XPosition, YPosition] = null;
    }

    public void TakeDamage(float delta) {
        OnHit?.Invoke(this, new IDamageable.OnHitEventArgs {
            Damage = delta
        });
    }

    public void EnableBlockObject() {
        BlockObject ??= World.Instance.CreateBlockObject(this);
    }

    public Direction GetDirectionOfBlock(Block target) {
        if (target is null) {
            throw new Exception("Block is null");
        }

        float xDelta = target.XPosition - XPosition;
        float yDelta = target.YPosition - YPosition;

        return (xDelta, yDelta) switch {
            (1, 0) => Direction.Right,
            (-1, 0) => Direction.Left,
            (0, -1) => Direction.Down,
            (0, 1) => Direction.Up,
            _ => throw new Exception("Blocks are not adjacent")
        };
    }

    public Block GetBlockInDirection(Direction direction) {
        return direction switch {
            Direction.Down => YPosition == 0
                ? null
                : GetBlockAtPosition(
                    XPosition, YPosition - 1),
            Direction.Up => YPosition == WORLD_HEIGHT - 1
                ? null
                : GetBlockAtPosition(
                    XPosition, YPosition + 1),
            Direction.Left => XPosition == 0
                ? null
                : GetBlockAtPosition(
                    XPosition - 1, YPosition),
            Direction.Right => XPosition == WORLD_WIDTH - 1
                ? null
                : GetBlockAtPosition(
                    XPosition + 1, YPosition),
            _ => throw new InvalidOperationException("invalid direction")
        };
    }

    public Dictionary<Direction, Block> GetAdjacentBlocks() {
        return new Dictionary<Direction, Block> {
            { Direction.Down, GetBlockInDirection(Direction.Down) },
            { Direction.Up, GetBlockInDirection(Direction.Up) },
            { Direction.Left, GetBlockInDirection(Direction.Left) },
            { Direction.Right, GetBlockInDirection(Direction.Right) },
        };
    }
}
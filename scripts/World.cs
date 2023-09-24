using System;
using System.Collections.Generic;
using Godot;

using static Godot.GD;

namespace TerrariaRipoffNNF.scripts;

public partial class World : Node2D {
    public static World Instance { get; private set; }

    private const int BLOCK_OBJECT_DISTANCE = 10;
    private const int BLOCK_SIZE = 50;

    private PackedScene blockPackedScene = Load<PackedScene>("res://scenes/block.tscn");
    private PackedScene playerPackedScene = Load<PackedScene>("res://scenes/player.tscn");
    

    public override void _Ready() {
        Instance = this;
        Player.OnPlayerSpawned += Player_OnPlayerSpawned;
        SpawnPlayer();
    }

    private void Player_OnPlayerSpawned(object sender, EventArgs eventArgs) {
        Player player = (Player)sender;

        Vector2 playerPosition = player.Position;
        int playerXCell = (int)playerPosition.X;
        int playerYCell = (int)playerPosition.Y;

        int xStartPosition = Math.Max(0, playerXCell / BLOCK_SIZE - BLOCK_OBJECT_DISTANCE);
        int yStartPosition = Math.Max(0, playerYCell / BLOCK_SIZE - BLOCK_OBJECT_DISTANCE);
        int xEndPosition = Math.Min(Block.WORLD_WIDTH, playerXCell / BLOCK_SIZE + BLOCK_OBJECT_DISTANCE);
        int yEndPosition = Math.Min(Block.WORLD_HEIGHT, playerYCell / BLOCK_SIZE + BLOCK_OBJECT_DISTANCE);

        List<Block> closeBlocks = Block.GetBlocksInArea(
            xStartPosition, yStartPosition, xEndPosition, yEndPosition);
        foreach (Block block in closeBlocks) {
            block.EnableBlockObject();
        }

        player.OnPlayerMovedCell += Player_OnPlayerMovedCell;
    }

    private void Player_OnPlayerMovedCell(object sender, Player.OnPlayerMovedCellEventArgs eventArgs) {
        Player player = (Player)sender;

        int playerXCell = (int)player.Position.X;
        int playerYCell = (int)player.Position.Y;

        int xStartPosition = Math.Max(0, playerXCell / BLOCK_SIZE + BLOCK_OBJECT_DISTANCE - 1);
        int yStartPosition = Math.Max(0, playerYCell / BLOCK_SIZE - BLOCK_OBJECT_DISTANCE);
        int xEndPosition = Math.Min(Block.WORLD_WIDTH, playerXCell / BLOCK_SIZE + BLOCK_OBJECT_DISTANCE);
        int yEndPosition = Math.Min(Block.WORLD_HEIGHT, playerYCell / BLOCK_SIZE + BLOCK_OBJECT_DISTANCE);
        
        List<Block> closeBlocks = Block.GetBlocksInArea(
            xStartPosition, yStartPosition, xEndPosition, yEndPosition);
        foreach (Block block in closeBlocks) {
            block.EnableBlockObject();
        }
    }

    private void SpawnPlayer() {
        var player = playerPackedScene.Instantiate<Player>();
        player.Position = new Vector2(50, -199);
        AddChild(player);
    }

    public BlockNode CreateBlockObject(Block block) {
        var instance = blockPackedScene.Instantiate<BlockNode>();
        instance.Block = block;
        instance.Position = new Vector2(block.XPosition * BLOCK_SIZE, block.YPosition * BLOCK_SIZE);
        AddChild(instance);
        return instance;
    }
}
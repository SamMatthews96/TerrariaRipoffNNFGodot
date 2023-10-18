using Godot;
using System;
using TerrariaRipoffNNF.scripts;
using TerrariaRipoffNNF.scripts.BlockScripts;
using static Godot.GD;
using BlockResource = TerrariaRipoffNNF.scripts.BlockScripts.BlockResource;

public partial class TestManualBlockSpawner : Node2D {
    [Export] private BlockResource testBlockResource;

    public override void _Process(double delta) {
        if (Input.IsActionPressed("leftMouse")) {
            (int xPosition, int yPosition) = GetMouseBlockCoordinates();
            if (xPosition is < 0 or >= Block.WORLD_WIDTH) return;
            if (yPosition is < 0 or >= Block.WORLD_HEIGHT) return;
            if (Block.GetBlockAtPosition(xPosition, yPosition) is null) {
                Rpc(nameof(RpcCreateBlock),xPosition, yPosition, testBlockResource.Name);
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcCreateBlock(int xPosition, int yPosition, string resourceId) {
        BlockResource resource = Load<BlockResource>($"res://BlockResources/{resourceId}.tres");
        Block.CreateBlock(xPosition, yPosition, resource);
    }

    private (int xPosition, int yPosition) GetMouseBlockCoordinates() {
        var mousePosition = GetGlobalMousePosition();
        return ((int)Math.Round(mousePosition.X / World.BLOCK_SIZE),
            (int)Math.Round(-mousePosition.Y / World.BLOCK_SIZE));
    }
}
using System;
using System.Diagnostics;
using Godot;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Scenes.Scripts;

public partial class ActiveBlock : StaticBody2D {
    [Export] private static PackedScene packedActiveBlock =
        ResourceLoader.Load<PackedScene>("res://Scenes/ActiveBlock.tscn");

    private int xPosition;
    private int yPosition;

    [Export] private Sprite2D sprite;
    private BlockType blockType;

    [Signal]
    public delegate void TakenDamageEventHandler(int xPosition, int yPosition, float damageAmount);

    public static ActiveBlock Instantiate(BlockType blockType, int xPosition, int yPosition) {
        ActiveBlock newBlock = packedActiveBlock.Instantiate<ActiveBlock>();
        newBlock.xPosition = xPosition;
        newBlock.yPosition = yPosition;
        newBlock.Position = new Vector2(
            xPosition * WorldManager.BLOCK_SIZE,
            yPosition * WorldManager.BLOCK_SIZE);
        newBlock.blockType = blockType;
        newBlock.sprite.Texture = blockType.Texture;
        return newBlock;
    }
}
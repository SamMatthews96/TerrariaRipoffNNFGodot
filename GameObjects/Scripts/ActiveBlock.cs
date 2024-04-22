using Godot;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.GameObjects.Scripts;

public partial class ActiveBlock : StaticBody2D {
    private static PackedScene packedActiveBlock =
        ResourceLoader.Load<PackedScene>("res://GameObjects/Scenes/ActiveBlock.tscn");

    private int _xPosition;
    private int _yPosition;

    [Export] private Sprite2D _sprite;
    private BlockType BlockType { get; set; }

    [Signal]
    public delegate void TakenDamageEventHandler(int xPosition, int yPosition, float damageAmount);

    public static ActiveBlock Instantiate(SavedBlock savedBlock) {
        ActiveBlock newBlock = packedActiveBlock.Instantiate<ActiveBlock>();
        newBlock._xPosition = savedBlock.XPosition;
        newBlock._yPosition = savedBlock.YPosition;
        newBlock.Position = new Vector2(
            savedBlock.XPosition * BlockManager.BLOCK_SIZE,
            savedBlock.YPosition * BlockManager.BLOCK_SIZE);
        newBlock.BlockType = savedBlock.BlockType;
        newBlock._sprite.Texture = savedBlock.BlockType.Texture;
        return newBlock;
    }

    private void OnInputEvent(Node _, InputEvent e, int __) {
        if (e is InputEventMouseButton mouseEvent) {
            EmitSignal(SignalName.TakenDamage, _xPosition, _yPosition, 100);
        }
    }
}
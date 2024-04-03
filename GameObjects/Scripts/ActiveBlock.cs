using Godot;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.GameObjects.Scripts;

public partial class ActiveBlock : StaticBody2D {
    [Export] private static PackedScene packedActiveBlock =
        ResourceLoader.Load<PackedScene>("res://GameObjects/Scenes/ActiveBlock.tscn");

    private int _xPosition;
    private int _yPosition;

    [Export] private Sprite2D _sprite;
    public BlockType BlockType { get; private set; }

    [Signal]
    public delegate void TakenDamageEventHandler(int xPosition, int yPosition, float damageAmount);

    public static ActiveBlock Instantiate(BlockType blockType, int xPosition, int yPosition) {
        ActiveBlock newBlock = packedActiveBlock.Instantiate<ActiveBlock>();
        newBlock._xPosition = xPosition;
        newBlock._yPosition = yPosition;
        newBlock.Position = new Vector2(
            xPosition * BlockManager.BLOCK_SIZE,
            yPosition * BlockManager.BLOCK_SIZE);
        newBlock.BlockType = blockType;
        newBlock._sprite.Texture = blockType.Texture;
        return newBlock;
    }

    private void OnInputEvent(Node _, InputEvent e, int __) {
        if (e is InputEventMouseButton mouseEvent) {
            EmitSignal(SignalName.TakenDamage, _xPosition, _yPosition, 100);
        }
    }
}
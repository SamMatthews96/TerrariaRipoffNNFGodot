using Godot;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.GameObjects.Scripts;

public partial class ActiveBlock : StaticBody2D {
    private static readonly PackedScene PackedScene =
        ResourceLoader.Load<PackedScene>("res://GameObjects/Scenes/ActiveBlock.tscn");

    public SavedBlock SavedBlock { get; private set; }

    private BlockType BlockType { get; set; }
    [Export] private Sprite2D _sprite;

    [Signal]
    public delegate void TakenDamageEventHandler(ActiveBlock activeBlock, float damageAmount);

    public static ActiveBlock Instantiate(SavedBlock savedBlock) {
        ActiveBlock newBlock = PackedScene.Instantiate<ActiveBlock>();
        newBlock.SavedBlock = savedBlock;
        newBlock.Position = new Vector2(
            savedBlock.XPosition * BlockManager.BLOCK_SIZE,
            savedBlock.YPosition * BlockManager.BLOCK_SIZE);
        newBlock.BlockType = savedBlock.BlockType;
        newBlock._sprite.Texture = savedBlock.BlockType.Texture;
        return newBlock;
    }

    private void OnInputEvent(Node _, InputEvent e, int __) {
        if (e is InputEventMouseButton mouseEvent) {
            EmitSignal(SignalName.TakenDamage, this, 100);
        }
    }
}
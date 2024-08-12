using Godot;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.GameObjects.Scripts;

public partial class ActiveBlock : StaticBody2D {
    public SavedBlock SavedBlock { get; private set; }

    [Export] private BlockType BlockType { get; set; }
    [Export] private Sprite2D _sprite;
    
    [Export] public int XCoordinate { get; private set; }
    [Export] public int YCoordinate { get; private set; }

    [Signal] public delegate void TakenDamageEventHandler(ActiveBlock activeBlock, float damageAmount);

    public void Initialize(SavedBlock savedBlock) {
        SavedBlock = savedBlock;
        XCoordinate = savedBlock.XPosition;
        YCoordinate = savedBlock.YPosition;
        Position = new Vector2(
            savedBlock.XPosition * HostBlockManager.BlockSize,
            savedBlock.YPosition * HostBlockManager.BlockSize);
        BlockType = savedBlock.BlockType;
        _sprite.Texture = savedBlock.BlockType.Texture;
    }

    private void OnInputEvent(Node _, InputEvent e, int __) {
        if (e is InputEventMouseButton mouseEvent) {
            // EmitSignal(SignalName.TakenDamage, this, 100);
        }
    }

 
}
using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.GameObjects.Scripts;

public partial class ActiveBlock : StaticBody2D {
    public SavedBlock SavedBlock { get; private set; }
    [Export] private Dictionary _savedBlockDictionary;

    [Export] private Sprite2D _sprite;

    [Signal] public delegate void TakenDamageEventHandler(ActiveBlock activeBlock, float damageAmount);

    public void Initialize(SavedBlock savedBlock) {
        HostManager.RequireHost();

        _savedBlockDictionary = savedBlock.Serialize();
    }

    public override void _Ready() {
        SavedBlock = SavedBlock.FromDict(_savedBlockDictionary);
        
        Position = new Vector2(
            SavedBlock.XPosition * HostBlockManager.BlockSize,
            SavedBlock.YPosition * HostBlockManager.BlockSize);
        _sprite.Texture = SavedBlock.BlockType.Texture;
    }

    private void OnInputEvent(Node _, InputEvent e, int __) {
        if (e is InputEventMouseButton mouseEvent) {
            // EmitSignal(SignalName.TakenDamage, this, 100);
        }
    }

 
}
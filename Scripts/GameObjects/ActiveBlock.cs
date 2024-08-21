using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Managers;
using TerrariaRipoffNNF.Scripts.Managers.Host;
using TerrariaRipoffNNF.Scripts.Resources;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public partial class ActiveBlock : StaticBody2D {
    public SavedBlock SavedBlock { get; private set; }
    [Export] private Dictionary _savedBlockDictionary;
    [Export] private Sprite2D _sprite;

    public void Initialize(SavedBlock savedBlock) {
        HostManager.RequireHost();

        _savedBlockDictionary = savedBlock.Serialize();
    }

    public override void _Ready() {
        SavedBlock = SavedBlock.FromDict(_savedBlockDictionary);

        Position = new Vector2(
            SavedBlock.XPosition * GameManager.BlockSize,
            SavedBlock.YPosition * GameManager.BlockSize);
        _sprite.Texture = SavedBlock.BlockType.Texture;
    }
}
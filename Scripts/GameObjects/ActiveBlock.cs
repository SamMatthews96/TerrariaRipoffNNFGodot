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


    public event Action<ActiveBlock, float> TakenDamage;

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

    private void OnInputEvent(Node _, InputEvent e, int __) {
        if (e is InputEventMouseButton mouseButton) {
            RpcId(Manager.MultiplayerHostId, nameof(ServerHandleTakeDamage), 100f);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerHandleTakeDamage(float damageAmount) {
        TakenDamage?.Invoke(this, damageAmount);
    }
}
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
            SavedBlock.XPosition * GameManager.BlockSize,
            SavedBlock.YPosition * GameManager.BlockSize);
        _sprite.Texture = SavedBlock.BlockType.Texture;
    }

    private void OnInputEvent(Node _, InputEvent e, int __) {
        if (e is InputEventMouseButton) {
            RpcId(Manager.MultiplayerHostId, nameof(ServerHandleTakeDamage), 100f);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerHandleTakeDamage(float damageAmount) {
        EmitSignal(SignalName.TakenDamage, this, damageAmount);
    }
}
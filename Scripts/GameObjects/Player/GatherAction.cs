using System;
using Godot;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class GatherAction : PlayerAction {
    [Export] private Player _player;
    public event Action<IntVector, float> GatherAttempted;

    private Game _game;

    private bool _isGathering;
    [Export] private Timer _gatherCooldown;

    public void InitAsLocal(Game game) {
        _game = game;
        _player.ActionController.ActionChanged += OnActionChanged;
        _gatherCooldown.Timeout += OnGatherCooldownTimeout;
    }

    private void OnGatherCooldownTimeout() {
        if (_isGathering) {
            AttemptGather();
        }
    }

    private void OnActionChanged(Type _) {
        _isGathering = false;
    }

    public override void PrimaryAction(Vector2 mouseWorldPosition) {
        _isGathering = true;
        if (_gatherCooldown.IsStopped()) {
            AttemptGather();
        }
    }

    private void AttemptGather() {
        _gatherCooldown.Start();
        IntVector coords = new(_player.GetGlobalMousePosition() / Game.BlockSize);
        if (!_game.IsInBounds(coords)) return;

        float miningPowerTemp = 10f;
        float miningRangeTemp = 8f;
        if (miningRangeTemp >= IntVector.Distance(coords, Player.Coords)) {
            RpcId(SceneManager.HostId, nameof(HostGatherAttempted),
                coords.ToSerialised(), miningPowerTemp);
            _gatherCooldown.Start();           
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void HostGatherAttempted(Array intVectorArray, float damage) {
        IntVector coords = new(intVectorArray);

        GatherAttempted?.Invoke(coords, damage);
    }

    public override void EndPrimaryAction(Vector2 mouseWorldPosition) {
        _isGathering = false;
    }
}
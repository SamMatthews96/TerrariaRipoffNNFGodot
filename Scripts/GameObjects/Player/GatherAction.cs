using System;
using Godot;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class GatherAction : PlayerAction {
    [Export] private Player _player;
    public event Action<IntVector, Player> GatherAttempted;

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

    private void OnActionChanged(PlayerActionType _) {
        _isGathering = false;
    }

    public override void PrimaryAction(Vector2 mouseWorldPosition) {
        _isGathering = true;
        if (_gatherCooldown.IsStopped()) {
            AttemptGather();
        }
    }

    private void AttemptGather() {
        IntVector coords = new(_player.GetGlobalMousePosition() / Game.BlockSize);
        if (!_game.IsInBounds(coords)) return;

        RpcId(SceneManager.HostId, nameof(HostAttemptGather), coords);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void HostAttemptGather(IntVector coords) {
        GatherAttempted?.Invoke(coords, _player);
    }

    public void OnAfterGatherSuccess() {
        _gatherCooldown.Start();
    }

    public override void EndPrimaryAction(Vector2 mouseWorldPosition) {
        _isGathering = false;
    }
}
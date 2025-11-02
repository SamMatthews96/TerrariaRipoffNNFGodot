using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class GatherAction : PlayerAction {
    public event Action<IntVector, Player> GatherAttempted;

    private bool _isGathering;
    [Export] private Timer _gatherCooldown;

    public override void _Ready() {
        Player = ActionController.Player;
        Game = ActionController.Game;
        Player.ActionController.ActionChanged += OnActionChanged;
        _gatherCooldown.Timeout += OnGatherCooldownTimeout;
    }
    
    public override void _ExitTree() {
        Player.ActionController.ActionChanged -= OnActionChanged;
        _gatherCooldown.Timeout -= OnGatherCooldownTimeout;
    }

    private void OnGatherCooldownTimeout() {
        if (_isGathering) {
            AttemptGather();
        }
    }

    private void OnActionChanged(PlayerActionType _) {
        _isGathering = false;
    }

    public override void LeftMouseAction(Vector2 mouseWorldPosition) {
        if (Player.PlayerEquipment.Pickaxe is null) return;
        _isGathering = true;
        if (_gatherCooldown.IsStopped()) {
            AttemptGather();
        }
    }

    public override void EndLeftMouseAction(Vector2 mouseWorldPosition) {
        _isGathering = false;
    }

    private void AttemptGather() {
        IntVector coords = new(Player.GetGlobalMousePosition() / Game.BlockSize);
        if (!Game.IsInBounds(coords)) return;

        GatherAttempted?.Invoke(coords, Player);
    }

    public void OnAfterGatherSuccess() {
        _gatherCooldown.Start();
    }
}
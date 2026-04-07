using System;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

namespace TerrariaRipoffNNF;

public partial class GatherAction : PlayerAction {
    public event Action<Vector2I, Player> GatherAttempted;

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
            MiningAnimation miningAnimation = MiningAnimation.Create();
            Player.AddChild(miningAnimation);
        }
    }

    public override void EndLeftMouseAction(Vector2 mouseWorldPosition) {
        _isGathering = false;
    }

    private void AttemptGather() {
        Vector2 temp = Player.GetGlobalMousePosition() / Game.BlockSize;
        Vector2I coords = new((int)temp.X, (int)temp.Y);
        
        if (!Game.World.IsInBounds(coords)) return;

        GatherAttempted?.Invoke(coords, Player);
    }

    public void OnAfterGatherSuccess() {
        _gatherCooldown.Start();
    }
}
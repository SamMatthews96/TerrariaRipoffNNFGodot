using System;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

namespace TerrariaRipoffNNF;

public partial class GatherAction : PlayerAction {
    public delegate void GatherActionDelegate(Vector2I coords, float damage);
    public event GatherActionDelegate HostGatherBlockAction;
    public event GatherActionDelegate HostGatherPropAction;
    public event GatherActionDelegate HostGatherWallAction;

    [Export] private Timer _gatherCooldown;

    public override void _Ready() {
        ProcessMode = ProcessModeEnum.Disabled;
        Player = ActionController.Player;

        if (!Player.IsLocalPlayer) return;
        Player.ActionController.ActionChanged += OnActionChanged;
        TreeExiting += () => { Player.ActionController.ActionChanged -= OnActionChanged; };
    }

    private void OnActionChanged(PlayerActionType _) {
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public override void LeftMouseAction(Vector2 mouseWorldPosition) {
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void EndLeftMouseAction(Vector2 mouseWorldPosition) {
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public override void _Process(double delta) {
        Vector2 mouseWorldPosition = Player.World.GetGlobalMousePosition();
        Vector2I targetCoords = (Vector2I)(mouseWorldPosition / Game.BlockSize);
        if (Player.World.IsHost) {
            AttemptGatherOnHost(targetCoords);
        } else {
            AttemptGatherOnClient(targetCoords);
        }
    }

    private void AttemptGatherOnClient(Vector2I targetCoords) {
        if (!_gatherCooldown.IsStopped()) return;
        int range = 8;
        if (!Player.World.IsInOrthogonalRange(
                targetCoords, Player.Coords, range)) return;
        _gatherCooldown.Start();
        RpcId(1, nameof(AttemptGatherOnHost), targetCoords);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void AttemptGatherOnHost(Vector2I targetCoords) {
        if (!_gatherCooldown.IsStopped()) return;

        int range = Player.PlayerEquipment.Pickaxe.Range;
        if (!Player.World.IsInOrthogonalRange(
                targetCoords, Player.Coords, range)) return;

        GatherActionDelegate action;
        if (Player.World.BlockManager.Blocks[
                targetCoords.X, targetCoords.Y] is not null) {
            action = HostGatherBlockAction;
        } else if (Player.World.PropManager.PropCells.ContainsKey(targetCoords)) {
            action = HostGatherPropAction;
        } else if (Player.World.BlockManager.Walls[
                       targetCoords.X, targetCoords.Y] is not null) {
            action = HostGatherWallAction;
        } else return;

        _gatherCooldown.Start();
        float damage = Player.PlayerEquipment.Pickaxe.Power;
        action?.Invoke(targetCoords, damage);
    }
}
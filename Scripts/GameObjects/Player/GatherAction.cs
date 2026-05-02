using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

namespace TerrariaRipoffNNF;

public partial class GatherAction : PlayerAction {
    public delegate void GatherActionDelegate(Vector2I coords, float damage);
    public event GatherActionDelegate HostGatheredBlock;
    public event GatherActionDelegate HostGatherProp;
    public event GatherActionDelegate HostGatheredWall;

    [Export] private Timer _gatherCooldown;
    private Array<CellEntity> _gatherTypes = new();

    public override void _Ready() {
        ProcessMode = ProcessModeEnum.Disabled;
        Player = ActionController.Player;
        _gatherTypes.Add(CellEntity.Block);
        _gatherTypes.Add(CellEntity.Prop);
        _gatherTypes.Add(CellEntity.Wall);

        if (!Player.IsLocalPlayer) return;
        Player.ActionState.ActionChanged += OnActionChanged;
        TreeExiting += () => { Player.ActionState.ActionChanged -= OnActionChanged; };
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

        CellEntity gatherType = Player.World.GetPriorityCellEntity(
            targetCoords, _gatherTypes
        );
        GatherActionDelegate action = gatherType switch {
            CellEntity.Block => HostGatheredBlock,
            CellEntity.Prop => HostGatherProp,
            CellEntity.Wall => HostGatheredWall,
            _ => null
        };
        if (action is null) return;

        _gatherCooldown.Start();
        float damage = Player.PlayerEquipment.Pickaxe.Power;
        action.Invoke(targetCoords, damage);
    }
}
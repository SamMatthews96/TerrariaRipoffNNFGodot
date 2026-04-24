using System;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

namespace TerrariaRipoffNNF;

public partial class GatherAction : PlayerAction {
    public event Action<Vector2I, float> ServerGatherAction;

    [Export] private Timer _gatherCooldown;

    public override void _Ready() {
        ProcessMode = ProcessModeEnum.Disabled;
        Player = ActionController.Player;
        if (!Player.IsLocalPlayer) return;
        Player.ActionController.ActionChanged += OnActionChanged;
    }

    public override void _ExitTree() {
        if (!Player.IsLocalPlayer) return;
        Player.ActionController.ActionChanged -= OnActionChanged;
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
        // runs while gather mode is active
        if (!_gatherCooldown.IsStopped()) return;
        Vector2 mouseWorldPosition = Player.World.GetGlobalMousePosition();
        Vector2I coords = (Vector2I)(mouseWorldPosition / Game.BlockSize);
        GD.Print(mouseWorldPosition);

        if (!Player.World.IsInBounds(coords)) return;
        float range = 8;
        if (Math.Abs(coords.X - Player.Coords.X) > range) return;
        if (Math.Abs(coords.Y - Player.Coords.Y) > range) return;

        if (Player.PlayerEquipment.Pickaxe is null) return;

        if (Player.World.IsHost) {
            if (Player.World.Blocks[coords.X, coords.Y] is null) return;
            _gatherCooldown.Start();
            float damage = Player.PlayerEquipment.Pickaxe.Power;
            ServerGatherAction?.Invoke(coords, damage);
        } else {
            _gatherCooldown.Start();
            RpcId(1, nameof(AttemptGatherOnHost), coords);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void AttemptGatherOnHost(Vector2I coords) {
        // the client has determined that they can gather
        if (!_gatherCooldown.IsStopped()) {
            GD.Print("Host Gather on cooldown");
            return;
        }

        if (!Player.World.IsInBounds(coords)) return;
        float range = 8;
        if (Math.Abs(coords.X - Player.Coords.X) > range) return;
        if (Math.Abs(coords.Y - Player.Coords.Y) > range) return;

        if (Player.PlayerEquipment.Pickaxe is null) return;

        if (Player.World.Blocks[coords.X, coords.Y] is null) return;

        _gatherCooldown.Start();
        float damage = Player.PlayerEquipment.Pickaxe.Power;
        ServerGatherAction?.Invoke(coords, damage);
    }

    public void OnAfterGatherSuccess() {
        _gatherCooldown.Start();
    }
}
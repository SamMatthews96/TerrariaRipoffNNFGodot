using System;
using Godot;

namespace TerrariaRipoffNNF;

public delegate void AttackActionDelegate(Vector2 position, Player player);

public partial class WeaponAction : PlayerAction {
    [Export] private Timer _weaponCooldown;

    public event AttackActionDelegate WeaponSwung;

    public override void _Ready() {
        Player = ActionController.Player;
    }

    public override void LeftMouseAction(Vector2 mouseWorldPosition) {
        ItemWeapon weapon = Player.PlayerEquipment.Weapon;
        if (weapon is null) return;
        if (!_weaponCooldown.IsStopped()) return;

        RpcId(Game.HostId, nameof(RpcAttack), mouseWorldPosition);
        _weaponCooldown.Start();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcAttack(Vector2 mouseWorldPosition) {
        WeaponSwung?.Invoke(mouseWorldPosition, Player);
    }

    public override void EndLeftMouseAction(Vector2 mouseWorldPosition) { }
}
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

        WeaponSwung?.Invoke(mouseWorldPosition, Player);
        _weaponCooldown.Start();
    }

    public override void EndLeftMouseAction(Vector2 mouseWorldPosition) { }
}
using Godot;

namespace TerrariaRipoffNNF;

public partial class WeaponAction : PlayerAction {
    [Export] private Timer _weaponCooldown;

    public override void _Ready() {
        Player = ActionController.Player;
    }

    public override void LeftMouseAction(Vector2 mouseWorldPosition) {
        ItemWeapon weapon = Player.PlayerEquipment.Weapon;
        if (weapon is null) return;
        if (!_weaponCooldown.IsStopped()) return;
        
        WeaponAttackNode.Create(weapon, Player, mouseWorldPosition);
        
        
        _weaponCooldown.Start();
    }

    public override void EndLeftMouseAction(Vector2 mouseWorldPosition) { }
}
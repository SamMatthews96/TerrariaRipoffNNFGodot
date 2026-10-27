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
        
        WeaponAttackNode node = 
            WeaponAttackNode.Create(weapon, Player, mouseWorldPosition);
        Player.World.AddChild(node);
        _weaponCooldown.Start();
    }

    public override void EndLeftMouseAction(Vector2 mouseWorldPosition) { }
}
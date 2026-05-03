using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

namespace TerrariaRipoffNNF;

public partial class WeaponAction : PlayerAction {
    [Export] private Timer _weaponCooldown;

    public override void _Ready() {
        Player = ActionController.Player;
    }

    public override void LeftMouseAction(Vector2 mouseWorldPosition) {
        if (Player.PlayerEquipment.Weapon is null) return;
        if (!_weaponCooldown.IsStopped()) return;
        // temporarily just reach through the tree,
        // listen to weapon changes
        // when weapon changes, set Weapon

        WeaponAttackNode proj = WeaponAttackNode.Create(
            Player.PlayerEquipment.Weapon, Player, mouseWorldPosition);
        Player.World.AddChild(proj);
        _weaponCooldown.Start();
    }

    public override void EndLeftMouseAction(Vector2 mouseWorldPosition) { }
}
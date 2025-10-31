using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

namespace TerrariaRipoffNNF;

public partial class WeaponAction : PlayerAction {
    [Export] private Timer _weaponCooldown;

    public override void _Ready() {
        Player = ActionController.Player;
        Game = ActionController.Game;
    }

    public override void LeftMouseAction(Vector2 mouseWorldPosition) {
        if (!_weaponCooldown.IsStopped()) return;
        PackedScene sprite = Player.PlayerEquipment.Weapon.TestProjectile;
        TestProjectile proj = TestProjectile.Create(
            sprite, Player.Position, mouseWorldPosition);
        Game.PlayerParent.AddChild(proj);
        _weaponCooldown.Start();
    }

    public override void EndLeftMouseAction(Vector2 mouseWorldPosition) { }
}
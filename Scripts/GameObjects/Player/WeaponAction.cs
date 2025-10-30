using Godot;

namespace TerrariaRipoffNNF;

public partial class WeaponAction : PlayerAction {

    private Timer _weaponCooldown;
    
    public override void _Ready() {
        Player = ActionController.Player;
        Game = ActionController.Game;
    }
    
    public override void LeftMouseAction(Vector2 mouseWorldPosition) {
        throw new System.NotImplementedException();
    }
    public override void EndLeftMouseAction(Vector2 mouseWorldPosition) {
        throw new System.NotImplementedException();
    }
}
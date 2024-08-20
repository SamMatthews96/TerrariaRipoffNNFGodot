using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF.Scripts.Actions;

public class WeaponActionState : IActionState {
    private Player _player;

    public WeaponActionState(Player player) {
        _player = player;
    }

    public void Equip() {
        
    }
    
    public void Unequip() {
        
    }

    public void PrimaryAction(Vector2 mouseWorldPosition) {
        GD.Print("weapon primary");
    }

    public void EndPrimaryAction(Vector2 mouseScreenPosition) {
        
    }
}
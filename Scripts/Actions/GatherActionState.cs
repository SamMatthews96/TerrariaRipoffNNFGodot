using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF.Scripts.Actions;

public class GatherActionState : IActionState {
    private Player _player;

    public GatherActionState(Player player) {
        _player = player;
    }

    public void Equip() {
        
    }
    
    public void Unequip() {
        
    }

    public void PrimaryAction(Vector2 mouseWorldPosition) {
        GD.Print(mouseWorldPosition);
        
    }

    public void EndPrimaryAction(Vector2 mouseWorldPosition) {
        
    }
}
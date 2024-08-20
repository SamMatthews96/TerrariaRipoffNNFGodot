using Godot;

namespace TerrariaRipoffNNF.Scripts.Actions;

public interface IActionState {
    public void Equip() { }
    

    public void Unequip() {
        
    }
    
    public void PrimaryAction(Vector2 mouseWorldPosition);
    
    public void EndPrimaryAction(Vector2 mouseScreenPosition);
}
using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public interface IActionState {
    public void Equip() { }
    

    public void Unequip() {
        
    }
    
    public void PrimaryAction(Vector2 mouseScreenPosition);
    
    public void EndPrimaryAction(Vector2 mouseScreenPosition);
}
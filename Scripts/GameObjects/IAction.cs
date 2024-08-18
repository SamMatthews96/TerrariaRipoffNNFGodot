using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public interface IAction {
    public void PrimaryAction(Player player, Vector2 mouseScreenPosition);
    
    public void EndPrimaryAction(Player player, Vector2 mouseScreenPosition);
}
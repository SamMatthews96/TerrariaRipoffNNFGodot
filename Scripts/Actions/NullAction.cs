using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF.Scripts.Actions;

public class NullAction : IAction{
    public void PrimaryAction(Player player, Vector2 mouseScreenPosition) {
        GD.Print("Null Action Start");
    }

    public void EndPrimaryAction(Player player, Vector2 mouseScreenPosition) {
        GD.Print("Null Action End");
    }
}
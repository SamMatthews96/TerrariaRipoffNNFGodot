using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF.Scripts.Actions;

public class NullAction : IAction {
    private Player _player;

    public NullAction(Player player) {
        _player = player;
    }

    public void Equip() {
        GD.Print("Null Action Equip");
    }

    public void PrimaryAction(Vector2 mouseScreenPosition) {
        GD.Print("Null Action Start");
        GD.Print(_player.GlobalPosition);
        GD.Print(mouseScreenPosition);
        GD.Print("the diff is ");
        GD.Print(mouseScreenPosition - _player.GlobalPosition);
    }

    public void EndPrimaryAction(Vector2 mouseScreenPosition) {
        GD.Print("Null Action End");
    }
}
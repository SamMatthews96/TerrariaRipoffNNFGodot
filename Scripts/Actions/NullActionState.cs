using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;
using TerrariaRipoffNNF.Scripts.Managers;

namespace TerrariaRipoffNNF.Scripts.Actions;

public class NullActionState : IActionState {
    private Player _player;

    public NullActionState(Player player) {
        _player = player;
    }

    public void Equip() {
        GD.Print("Null Action Equip");
    }

    public void PrimaryAction(Vector2 mouseScreenPosition) {
        GD.Print("Null Action Start");
    }

    public void EndPrimaryAction(Vector2 mouseScreenPosition) {
        GD.Print("Null Action End");
    }
}
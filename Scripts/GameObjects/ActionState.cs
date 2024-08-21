using System;
using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public partial class ActionState : Node {
    public event Action Equipped;
    public event Action Unequipped;
    public event Action<Vector2> PrimaryActionStarted;
    public event Action<Vector2> PrimaryActionEnded;

    public void Equip() {
        Equipped?.Invoke();
    }

    public void Unequip() {
        Unequipped?.Invoke();
    }

    public void PrimaryAction(Vector2 mouseWorldPosition) {
        PrimaryActionStarted?.Invoke(mouseWorldPosition);
    }

    public void EndPrimaryAction(Vector2 mouseWorldPosition) {
        PrimaryActionEnded?.Invoke(mouseWorldPosition);
    }
}
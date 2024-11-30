using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class ActionState : Node {
    public event Action EnteredState;
    public event Action LeftState;
    public event Action<Vector2> PrimaryActionStarted;
    public event Action<Vector2> PrimaryActionEnded;

    public void EnterState() {
        EnteredState?.Invoke();
    }

    public void LeaveState() {
        LeftState?.Invoke();
    }

    public void PrimaryAction(Vector2 mouseWorldPosition) {
        PrimaryActionStarted?.Invoke(mouseWorldPosition);
    }

    public void EndPrimaryAction(Vector2 mouseWorldPosition) {
        PrimaryActionEnded?.Invoke(mouseWorldPosition);
    }
}
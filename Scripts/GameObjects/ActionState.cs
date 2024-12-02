using System;
using Godot;

namespace TerrariaRipoffNNF;
    
public enum PlayerActionState {
    Gather,
    Build,
    Weapon
}

public partial class ActionState : Node {
    [Export] public PlayerActionState State { get; private set; }
    public event Action<Vector2> PrimaryActionStarted;
    public event Action<Vector2> PrimaryActionEnded;
    
    public void PrimaryAction(Vector2 mouseWorldPosition) {
        PrimaryActionStarted?.Invoke(mouseWorldPosition);
    }

    public void EndPrimaryAction(Vector2 mouseWorldPosition) {
        PrimaryActionEnded?.Invoke(mouseWorldPosition);
    }
}
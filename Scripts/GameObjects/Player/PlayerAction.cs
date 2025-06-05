using Godot;

namespace TerrariaRipoffNNF;

public abstract partial class PlayerAction : Node {
    [Export] protected Player Player { get; private set; }
    [Export] public PlayerActionType State { get; private set; }


    public abstract void PrimaryAction(Vector2 mouseWorldPosition);

    public abstract void EndPrimaryAction(Vector2 mouseWorldPosition);
}
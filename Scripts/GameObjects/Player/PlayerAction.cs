using Godot;

namespace TerrariaRipoffNNF;

public abstract partial class PlayerAction : Node {
    [Export] protected Player Player { get; private set; }
    [Export] public PlayerActionType State { get; private set; }


    public abstract void LeftMouseAction(Vector2 mouseWorldPosition);

    public abstract void EndLeftMouseAction(Vector2 mouseWorldPosition);

    public virtual void RightMouseAction(Vector2 mouseWorldPosition){}

    public virtual void EndRightMouseAction(Vector2 mouseWorldPosition){}
}
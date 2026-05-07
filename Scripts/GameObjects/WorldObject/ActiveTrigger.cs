using System;
using Godot;

namespace TerrariaRipoffNNF;

public abstract partial class ActiveTrigger : StaticBody2D {
    public event Action Triggered;

    protected void Trigger() {
        Triggered?.Invoke();
    }
}
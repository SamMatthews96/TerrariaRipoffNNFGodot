using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class ActiveButton : StaticBody2D {
    public event Action Triggered;
    
    public override void _Ready() {
        InputEvent += (_, inputEvent, _) => {
            if (inputEvent is not InputEventMouseButton buttonEvent) return;
            if (buttonEvent.GetButtonIndex() != MouseButton.Right) return;
            if (!buttonEvent.Pressed) return;
            Triggered?.Invoke();
        };
    }
}
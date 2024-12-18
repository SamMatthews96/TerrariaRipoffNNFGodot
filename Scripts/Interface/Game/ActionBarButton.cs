using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class ActionBarButton : TextureButton {
    [Export] public PlayerAction.Type State { get; private set; }
    
    public event Action<PlayerAction.Type> ButtonClicked; 

    public override void _Ready() {
        Pressed += () => ButtonClicked?.Invoke(State);
    }

    public void SetFocus() {
        Modulate = new Color(1, 1, 1);
    }
    
    public void SetDefocus() {
        Modulate = new Color(1, 1, 1, 0.5f);
    }
    
    
}
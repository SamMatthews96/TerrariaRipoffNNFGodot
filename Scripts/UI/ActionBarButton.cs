using Godot;
using State = TerrariaRipoffNNF.ActionState.State;

namespace TerrariaRipoffNNF;

public partial class ActionBarButton : TextureButton {
    public State State { get; private set; } 

    public void Initialize(Texture2D texture2D, State state) {
        TextureNormal = texture2D;
        State = state;
    }

    public override void _Ready() { }
}
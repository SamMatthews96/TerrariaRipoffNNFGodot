using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF.Scripts.UI;

public partial class ActionBarButton : TextureButton {
    private IAction _action;
    
    public void Initialize(IAction action) {
        _action = action;
    }
}
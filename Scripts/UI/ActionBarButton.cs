using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF.Scripts.UI;

public partial class ActionBarButton : TextureButton {
    
    public void Initialize(Texture2D texture2D) {
        TextureNormal = texture2D;
    }
    
    public override void _Ready() {
    }
}
using Godot;

namespace TerrariaRipoffNNF;

public partial class ActionBarButton : TextureButton {
    
    public void Initialize(Texture2D texture2D) {
        TextureNormal = texture2D;
    }
    
    public override void _Ready() {
    }
}
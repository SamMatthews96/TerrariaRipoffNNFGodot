using Godot;

namespace TerrariaRipoffNNF.Scripts.Interface;

public partial class LoadingScreen : Node {
    [Export] private Sprite2D _loadingSprite;
    
    public override void _Process(double delta) {
        _loadingSprite.RotationDegrees += (float)delta * 120;
    }
}
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class PlayerEquipmentSlot : TextureButton {
    [Export] private Texture2D _itemIcon;

    public override void _Ready() {
        TextureNormal = _itemIcon;
        Pressed += OnPressed;
    }

    public override void _ExitTree() {
        Pressed -= OnPressed;
    }

    private void OnPressed() {
        TextureNormal = _itemIcon;
    }
}
using System;
using Godot;

namespace TerrariaRipoffNNF.Scripts.UI;

public partial class ActionBar : PanelContainer {
    [Export] private PackedScene _actionBarButtonScene;
    [Export] private HBoxContainer _buttonContainer;

    [Export] private TextureButton _gatherButton;
    [Export] private TextureButton _buildButton;
    [Export] private TextureButton _weaponButton;

    public event Action<int> ButtonClicked;

    public override void _Ready() {
        _gatherButton.Pressed += () => OnButtonClicked(0);
        _buildButton.Pressed += () => OnButtonClicked(1);
        _weaponButton.Pressed += () => OnButtonClicked(2);
    }

    private void OnButtonClicked(int index) {
        ButtonClicked?.Invoke(index);
    }
}
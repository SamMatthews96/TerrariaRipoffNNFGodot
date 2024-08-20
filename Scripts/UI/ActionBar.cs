using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Scripts.UI;

public partial class ActionBar : PanelContainer {
    [Export] private PackedScene _actionBarButtonScene;
    [Export] private HBoxContainer _buttonContainer;

    private List<ActionBarButton> _actionBarButtons = new();


    public event Action<int> ButtonClicked;

    public void Initialize(List<Texture2D> actionIcons) {
        foreach (Texture2D actionIcon in actionIcons) {
            ActionBarButton actionBarButton = _actionBarButtonScene.Instantiate<ActionBarButton>();
            _buttonContainer.AddChild(actionBarButton);
            actionBarButton.Initialize(actionIcon);
            int index = _actionBarButtons.Count;
            actionBarButton.Pressed += () => OnButtonClicked(index);
            _actionBarButtons.Add(actionBarButton);
        }
    }

    private void OnButtonClicked(int index) {
        ButtonClicked?.Invoke(index);
    }
}
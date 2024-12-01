using System;
using Godot;
using Godot.Collections;


namespace TerrariaRipoffNNF;

public partial class ActionBar : PanelContainer {
    [Export] private PackedScene _actionBarButtonScene;
    [Export] private HBoxContainer _buttonContainer;
    
    [Export] private Dictionary<string, ActionBarButton> _actionBarButtons;

    public event Action<int> ButtonClicked;

    public override void _Ready() {
        // _gatherButton.Pressed += () => OnButtonClicked(0);
        // _buildButton.Pressed += () => OnButtonClicked(1);
        // _weaponButton.Pressed += () => OnButtonClicked(2);
        
        // for each State, create a button,
        // when button clicked, invoke ButtonClicked event with the index of the button
        
        foreach (ActionBarButton button in _actionBarButtons.Values) {
            button.Pressed += () => OnButtonClicked((int) button.State);
        }
    }

    private void OnButtonClicked(int index) {
        ButtonClicked?.Invoke(index);
    }
}
using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Interface;

public partial class ActionBar : PanelContainer {
    [Export] private Array<ActionBarButton> _buttons;
    
    public event Action<PlayerActionType> ButtonClicked;
    
    public override void _Ready() {
        foreach (ActionBarButton button in _buttons) {
            button.Pressed += () => {
                ButtonClicked?.Invoke(button.State);
            };
        }
        
        Player.LocalPlayerSpawned += OnLocalPlayerSpawned;
    }
    
    private void OnLocalPlayerSpawned(Player player) {
        player.ActionController.ActionChanged += OnPlayerActionChanged;
    }
    
    private void OnPlayerActionChanged(PlayerActionType state) {
        
        foreach (ActionBarButton button in _buttons) {
            if (button.State == state) {
                button.SetFocus();
            } else {
                button.SetDefocus();
            }
        }
    }
    
    public override void _ExitTree() {
        Player.LocalPlayerSpawned -= OnLocalPlayerSpawned;
    }
}
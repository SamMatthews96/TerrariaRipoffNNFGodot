using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class ActionBar : PanelContainer {
    [Export] private Array<ActionBarButton> _buttons;
    
    public event Action<PlayerActionState> ButtonClicked;
    
    public override void _Ready() {
        foreach (ActionBarButton button in _buttons) {
            button.Pressed += () => {
                ButtonClicked?.Invoke(button.State);
            };
        }
        
        Player.BeforeLocalPlayerSpawned += OnBeforeLocalPlayerSpawned;
    }
    
    private void OnBeforeLocalPlayerSpawned(Player player) {
        player.ActionController.ActionChanged += OnPlayerActionChanged;
    }
    
    private void OnPlayerActionChanged(PlayerActionState state) {
        foreach (ActionBarButton button in _buttons) {
            if (button.State == state) {
                button.SetFocus();
            } else {
                button.SetDefocus();
            }
        }
    }
}
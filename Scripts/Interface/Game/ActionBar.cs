using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Interface;

public partial class ActionBar : PanelContainer {
    [Export] private Array<ActionBarButton> _buttons;
    [Export] private Game _gameInterface;
    
    public event Action<PlayerActionType> ButtonClicked;
    
    public override void _Ready() {
        foreach (ActionBarButton button in _buttons) {
            button.Pressed += () => {
                ButtonClicked?.Invoke(button.State);
            };
        }
        
        _gameInterface.World.PlayerManager.LocalPlayerSpawned += OnLocalPlayerSpawned;
        TreeExiting += () => {
            _gameInterface.World.PlayerManager.LocalPlayerSpawned -= OnLocalPlayerSpawned;
        };
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
}
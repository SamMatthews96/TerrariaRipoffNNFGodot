using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlayerSelectMenu : Control {
    [Export] private MainMenu _mainMenu;
    [Export] private Button _createPlayerButton;
    [Export] private Button _backButton; 
    [Export] private Control _playerListContainer;

    public event Action CreatePlayerButtonDown;
    public event Action BackButtonDown;

    public override void _Ready() {
        Hide();
        _createPlayerButton.ButtonDown += OnCreatePlayerButtonDown;
        _backButton.ButtonDown += OnBackButtonDown;
        
        // get player objects
        // add player objects to player list container
    }

    private void OnCreatePlayerButtonDown() {
        // create player object
        // save player object
        // add player object to player list container
    }

    private void OnBackButtonDown() {
        Hide();
        BackButtonDown?.Invoke();
    }
    
    private void OnPlayerDictionaryLoaded(Dictionary playerDictionary) {
        // create player object
        // add player object to player list container
    }
}
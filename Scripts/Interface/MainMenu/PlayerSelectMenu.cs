using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlayerSelectMenu : Control {
    [Export] private MainMenu _mainMenu;
    [Export] private Button _createPlayerButton;
    [Export] private Button _backButton; 
    [Export] private Control _playerListContainer;
    [Export] private TextEdit _playerNameTextEdit;
    [Export] private PackedScene _packedPlayerSelectButton;
    
    public event Action BackButtonDown;
    public event Action<Dictionary> SelectPlayerButtonDown;

    public override void _Ready() {
        Hide();
        _createPlayerButton.ButtonDown += OnCreatePlayerButtonDown;
        _backButton.ButtonDown += OnBackButtonDown;
        
        Dictionary[] playerBasicInfoArray =  FileManager.LoadAllPlayerBasicData();
        foreach (Dictionary playerBasicInfo in playerBasicInfoArray) {
            AddSelectPlayerButton(playerBasicInfo);
        }
    }

    public override void _ExitTree() {
        _createPlayerButton.ButtonDown -= OnCreatePlayerButtonDown;
        _backButton.ButtonDown -= OnBackButtonDown;
    }

    private void OnSelectPlayerButtonDown(Dictionary playerDictionary) {
        Hide();
        SelectPlayerButtonDown?.Invoke(playerDictionary);
    }

    private void OnCreatePlayerButtonDown() {
        Dictionary newPlayer = new();
        newPlayer.Add("Name", _playerNameTextEdit.Text);
        AddSelectPlayerButton(newPlayer);
        FileManager.SavePlayer(newPlayer);
    }

    private void OnBackButtonDown() {
        Hide();
        BackButtonDown?.Invoke();
    }
    
    private void AddSelectPlayerButton(Dictionary playerDictionary) {
        PlayerListItem playerListItem = _packedPlayerSelectButton.Instantiate<PlayerListItem>();
        playerListItem.Initialize(playerDictionary);
        playerListItem.SelectPlayerButtonDown += OnSelectPlayerButtonDown;
        _playerListContainer.AddChild(playerListItem);
    }
}
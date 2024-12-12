using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlayerListItem : Control {
    private Dictionary _playerBasicInfo;
    [Export] private Label _playerNameLabel;
    [Export] private Button _enterPlayerButton;
    [Export] private Button _deletePlayerButton;

    public event Action<Dictionary> SelectPlayerButtonDown;
    public event Action<Dictionary> DeletePlayerButtonDown;
    
    public void Initialize(Dictionary playerBasicInfo) {
        _playerBasicInfo = playerBasicInfo;
        _playerNameLabel.Text = playerBasicInfo["Name"].ToString();
    }

    public override void _Ready() {
        _enterPlayerButton.ButtonDown += OnEnterPlayerButtonDown;
        _deletePlayerButton.ButtonDown += OnDeletePlayerButtonDown;
    }

    private void OnEnterPlayerButtonDown() {
        SelectPlayerButtonDown?.Invoke(_playerBasicInfo);
    }
    
    private void OnDeletePlayerButtonDown() {
        FileManager.DeletePlayer(_playerBasicInfo);
        DeletePlayerButtonDown?.Invoke(_playerBasicInfo);
        QueueFree();
    }
}
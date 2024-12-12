using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class MainMenu : Control {
    [Export] private SelectGameTypeMenu _selectGameTypeMenu;
    [Export] private MultiplayerMenu _multiplayerMenu;
    [Export] private WorldMenu _worldMenu;
    [Export] private PlayerSelectMenu _playerSelectMenu;
    [Export] private PackedScene _packedWorldListItem;

    private GameType _gameType;
    private bool _isWorldLoaded;
    private Dictionary _selectedWorld;
    private bool _isPlayerSelected;
    private Dictionary _selectedPlayer;

    private enum GameType {
        SinglePlayer,
        Host,
        Client
    }

    public event Action<Dictionary, Dictionary> SinglePlayerClickedEnterWorld;
    public event Action<Dictionary, Dictionary> HostClickedEnterWorld;
    public event Action<string, Dictionary> ClientClickedEnterWorld;

    public override void _Ready() {
        _selectGameTypeMenu.SinglePlayerButtonDown += OnMainMenuSinglePlayerButtonDown;
        _selectGameTypeMenu.MultiplayerButtonDown += OnMainMenuMultiplayerButtonDown;

        _worldMenu.SelectWorldButtonDown += OnWorldMenuSelectWorldButtonDown;
        _worldMenu.BackButtonDown += OnWorldMenuBackButtonDown;

        _multiplayerMenu.HostButtonDown += OnMultiplayerMenuHostButtonDown;
        _multiplayerMenu.JoinButtonDown += OnMultiplayerMenuJoinButtonDown;
        _multiplayerMenu.BackButtonDown += OnMultiplayerMenuBackButtonDown;

        _playerSelectMenu.BackButtonDown += OnPlayerSelectBackButtonDown;
        _playerSelectMenu.SelectPlayerButtonDown += OnPlayerSelectButtonDown;
    }

    #region MenuMenu EventHandlers

    private void OnMainMenuSinglePlayerButtonDown() {
        _gameType = GameType.SinglePlayer;
        _worldMenu.Show();
    }

    private void OnMainMenuMultiplayerButtonDown() {
        _multiplayerMenu.Show();
    }

    #endregion

    #region WorldMenu EventHandlers

    private void OnWorldMenuBackButtonDown() {
        switch (_gameType) {
            case GameType.SinglePlayer:
                _selectGameTypeMenu.Show();
                break;
            case GameType.Host:
                _multiplayerMenu.Show();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnWorldMenuSelectWorldButtonDown(WorldBasicInfo worldBasicInfo) {
        Task.Run(() => {
            _selectedWorld = FileManager.LoadWorld(worldBasicInfo);
            _isWorldLoaded = true;
            if (_isPlayerSelected) {
                LaunchGame();
            }
        });

        _playerSelectMenu.Show();
    }

    #endregion

    #region MultiplayerMenu EventHandlers

    private void OnMultiplayerMenuHostButtonDown() {
        _gameType = GameType.Host;
        _worldMenu.Show();
    }

    private void OnMultiplayerMenuJoinButtonDown() {
        _gameType = GameType.Client;
        _playerSelectMenu.Show();
    }

    private void OnMultiplayerMenuBackButtonDown() {
        _selectGameTypeMenu.Show();
    }

    #endregion


    private void OnPlayerSelectBackButtonDown() {
        switch (_gameType) {
            case GameType.SinglePlayer:
                _worldMenu.Show();
                break;
            case GameType.Host:
                _worldMenu.Show();
                break;
            case GameType.Client:
                _multiplayerMenu.Show();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    private void OnPlayerSelectButtonDown(Dictionary playerInfo) {
        _selectedPlayer = playerInfo;
        if (_isWorldLoaded || _gameType == GameType.Client) {
            LaunchGame();
        }
    }

    private void LaunchGame() {
        switch (_gameType) {
            case GameType.SinglePlayer:
                SinglePlayerClickedEnterWorld?.Invoke(_selectedWorld, _selectedPlayer);
                break;
            case GameType.Host:
                HostClickedEnterWorld?.Invoke(_selectedWorld, _selectedPlayer);
                break;
            case GameType.Client:
                ClientClickedEnterWorld?.Invoke("127.0.0.1", _selectedWorld);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
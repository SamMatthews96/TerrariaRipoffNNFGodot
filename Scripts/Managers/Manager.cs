using System;
using Godot;
using Godot.Collections;
using MainMenu = TerrariaRipoffNNF.Scripts.UI.MainMenu;
using PlayerInfo = TerrariaRipoffNNF.Scripts.Resources.PlayerInfo;

namespace TerrariaRipoffNNF.Scripts.Managers;

public partial class Manager : Node {
    public const int MultiplayerHostId = 1;

    [Export] private int port = 8910;
    [Export] private string address = "127.0.0.1";
    [Export] private PackedScene _gameManagerPackedScene;
    [Export] private MainMenu _mainMenu;
    
    private ENetMultiplayerPeer _peer;

    [Signal] public delegate void ConnectedToServerEventHandler(PlayerInfo playerInfo);

    [Signal] public delegate void BeforePlayerSpawnedEventHandler(PlayerInfo playerInfo);
    
    public override void _Ready() {
        _mainMenu.SinglePlayerClickedEnterWorld += OnMainMenuSinglePlayerClickedEnterWorld;
        _mainMenu.HostClickedEnterWorld += OnMainMenuHostClickedEnterWorld;
        _mainMenu.ClientEnteredWorld += OnMainMenuClientClickedEnterWorld;
    }

    private void OnMainMenuSinglePlayerClickedEnterWorld(Dictionary world, PlayerInfo playerInfo) {
        LaunchGame(playerInfo, world);
    }

    private void OnMainMenuHostClickedEnterWorld(Dictionary world, PlayerInfo playerInfo) {
        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateServer(port);
        if (error != Error.Ok) {
            throw new Exception("error cannot host! [20240808.1336.1] :" + error);
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = _peer;

        LaunchGame(playerInfo, world);
    }

    private void OnMainMenuClientClickedEnterWorld(string ip, PlayerInfo playerInfo) {
        Multiplayer.ConnectedToServer += () => { LaunchGame(playerInfo); };

        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateClient(ip, port);
        if (error != Error.Ok) {
            throw new Exception("error cannot join! [20240808.1337.1] :" + error);
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = _peer;
    }

    private void LaunchGame(PlayerInfo playerInfo, Dictionary world = null) {
        GameManager gameManager = _gameManagerPackedScene.Instantiate<GameManager>();
        AddChild(gameManager);
        gameManager.Initialize(playerInfo, world);
        
        _mainMenu.QueueFree();
    }
}
using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.UI.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class Manager : Node {
    [Export] private int port = 8910;
    [Export] private string address = "127.0.0.1";
    private ENetMultiplayerPeer peer;
    public const int MultiplayerHostId = 1;
    [Export] public PackedScene GameManagerScene { get; private set; }
    [Export] public PackedScene MultiplayerManagerScene { get; private set; }
    [Export] public PackedScene HostManagerScene { get; private set; }
    
    public static Manager Instance { get; private set; }

    [Signal]
    public delegate void ConnectedToServerEventHandler(PlayerInfo playerInfo);
    
    [Signal] public delegate void BeforePlayerSpawnedEventHandler(PlayerInfo playerInfo);
    
    public override void _EnterTree() {
        Instance = this;
    }

    public override void _Ready() {
        MainMenu.Instance.SinglePlayerClickedEnterWorld += OnMainMenuSinglePlayerClickedEnterWorld;
        MainMenu.Instance.HostClickedEnterWorld += OnMainMenuHostClickedEnterWorld;
        MainMenu.Instance.ClientEnteredWorld += OnMainMenuClientClickedEnterWorld;
    }
        
    // at the end of all these functions
    // the game world needs to be created 
    // the player character needs to be created 

    private void OnMainMenuHostClickedEnterWorld(Dictionary world, PlayerInfo playerInfo) {
        
        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateServer(port);
        if (error != Error.Ok) {
            GD.Print("error cannot host! :" + error);
            return;
        }

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;

        Multiplayer.PeerConnected += id => {
            RpcId(id, "");
        };
        
    }

    private void OnMainMenuSinglePlayerClickedEnterWorld(Dictionary world, PlayerInfo playerInfo) {
        HostManager.Instantiate(world);
        GD.Print(HostManager.Instance.Width);

        // spawn local area around player character
        // on before player spawned
        // spawn player character
    }

    private void OnMainMenuClientClickedEnterWorld(string ip, PlayerInfo playerInfo) {
        Multiplayer.ConnectedToServer += () => {
            // EmitSignal(SignalName.ConnectedToServer, playerInfo);
        };

        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(ip, port);
        if (error != Error.Ok) {
            GD.Print("error cannot join! :" + error);
            return;
        }

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
        
    }
}
using Godot;
using static Godot.GD;

namespace TerrariaRipoffNNF.scripts; 

public partial class MultiplayerController : Control {
    [Export] private int port = 8910;
    [Export] private string address = "127.0.0.1";
    private ENetMultiplayerPeer peer;

    private string sceneDirectory = "res://scenes/world.tscn";
    private string testBlockSpawnerDirectory = "res://gameObjects/testBlockSpawner.tscn";

    [Export] private Button hostButton;
    [Export] private Button joinButton;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;

        hostButton.Pressed += OnHostButtonPressed;
        joinButton.Pressed += OnJoinButtonPressed;
    }

    private void PeerConnected(long id) {
        Print("Player Connected " + id);
    }

    private void PeerDisconnected(long id) {
        Print("Player Disconnected");
    }

    private void ConnectedToServer() {
        Print("Connected to Server");
        Print(Multiplayer.IsServer());

    }

    private void ConnectionFailed() {
        Print("Disconnected from Server");
    }

    private void OnHostButtonPressed() {
        peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(port);
        if (error != Error.Ok) {
            Print("error cannot host! :" + error);
            return;
        }

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

        Multiplayer.MultiplayerPeer = peer;
        Print("Hosting");

        HostGame();
    }

    private void OnJoinButtonPressed() {
        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(address, port);
        if (error != Error.Ok) {
            Print("error cannot join! :" + error);
            return;
        }

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
        Print("Joining Game!");
        
        JoinGame();
    }
    
    private void HostGame() {
        Node2D scene = ResourceLoader.Load<PackedScene>(sceneDirectory).Instantiate<Node2D>();
        GetTree().Root.AddChild(scene);
        Node2D worldLoader = ResourceLoader.Load<PackedScene>(testBlockSpawnerDirectory).Instantiate<Node2D>();
        GetTree().Root.AddChild(worldLoader);
        Hide();
    }

    private void JoinGame() {
        Node2D scene = ResourceLoader.Load<PackedScene>(sceneDirectory).Instantiate<Node2D>();
        GetTree().Root.AddChild(scene);
        Hide();
    }

    
}
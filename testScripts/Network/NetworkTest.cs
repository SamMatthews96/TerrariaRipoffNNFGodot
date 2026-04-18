using Godot;

namespace TerrariaRipoffNNF;

public partial class NetworkTest : Node {
    private const int Port = 9999;
    private const string Address = "127.0.0.1";

    [Export] private PackedScene _networkTestScene;
    
    public override void _Ready() {
        CreateUI();
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
    }

    private void OnConnectedToServer() {
        GD.Print("Connected to server, instantiating scene");
        if (!Multiplayer.IsServer()) return;
        InstantiateNetworkScene();
    }

    private void OnServerDisconnected() {
        GD.Print("Disconnected from server");
    }

    private void InstantiateNetworkScene() {
        if (_networkTestScene == null) {
            GD.PrintErr("Network test scene is not assigned");
            return;
        }
        var instance = _networkTestScene.Instantiate();
        instance.Name = "NetworkTestScene";
        AddChild(instance);
    }

    private void CreateUI() {
        var hostButton = new Button {
            Text = "Host",
            Position = new Vector2(100, 100)
        };
        hostButton.Pressed += OnHostPressed;
        AddChild(hostButton);

        var joinButton = new Button {
            Text = "Join",
            Position = new Vector2(100, 150)
        };
        joinButton.Pressed += OnJoinPressed;
        AddChild(joinButton);
    }

    private void OnHostPressed() {
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(Port, 2);
        if (error != Error.Ok) {
            GD.PrintErr($"Failed to create server: {error}");
            return;
        }
        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Server started on port " + Port);
        InstantiateNetworkScene();
    }

    private void OnJoinPressed() {
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateClient(Address, Port);
        if (error != Error.Ok) {
            GD.PrintErr($"Failed to connect to server: {error}");
            return;
        }
        Multiplayer.MultiplayerPeer = peer;
        GD.Print($"Connecting to {Address}:{Port}");
    }
}
using Godot;
using static Godot.GD;
using TerrariaRipoffNNF.scripts.BlockScripts;

namespace TerrariaRipoffNNF.scripts;

public partial class MultiplayerController : Control {
    [Export] private int port = 8910;
    [Export] private string address = "127.0.0.1";
    private ENetMultiplayerPeer peer;

    private string sceneDirectory = "res://scenes/world.tscn";
    private string testBlockSpawnerDirectory = "res://gameObjects/testBlockSpawner.tscn";
    private string playerDirectory = "res://gameObjects/player.tscn";

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
        if (!Multiplayer.IsServer()) return;
        
        string[] resourceIds = new string[Block.WORLD_WIDTH * Block.WORLD_HEIGHT];

        for (int x = 0; x < Block.WORLD_WIDTH; x++) {
            for (int y = 0; y < Block.WORLD_HEIGHT; y++) {
                Block block = Block.GetBlockAtPosition(x, y);
                if (block is null) continue;
                string resourceId = block.BlockResource.Name;
                resourceIds[x * Block.WORLD_WIDTH + y] = resourceId;
            }
        }

        RpcId(id, nameof(JoinGame), resourceIds);

        
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void JoinGame(string[] resourceIds) {
        Node2D scene = ResourceLoader.Load<PackedScene>(sceneDirectory).Instantiate<Node2D>();
        GetTree().Root.AddChild(scene);
        Hide();
        
        int xPosition = 0;
        int yPosition = 0;
        foreach (string resourceId in resourceIds) {
            if (resourceId != "") {
                BlockResource resource = Load<BlockResource>($"res://BlockResources/{resourceId}.tres");
                Block.CreateBlock(xPosition, yPosition, resource);
            }

            yPosition++;
            if (yPosition == Block.WORLD_HEIGHT) {
                xPosition++;
                yPosition = 0;
            }
        }
        
        Node2D player = ResourceLoader.Load<PackedScene>(playerDirectory).Instantiate<Node2D>();
        scene.AddChild(player);
        Node2D player2 = ResourceLoader.Load<PackedScene>(playerDirectory).Instantiate<Node2D>();
        scene.AddChild(player2);
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

        Node2D scene = ResourceLoader.Load<PackedScene>(sceneDirectory).Instantiate<Node2D>();
        GetTree().Root.AddChild(scene);
        Node2D worldLoader = ResourceLoader.Load<PackedScene>(testBlockSpawnerDirectory).Instantiate<Node2D>();
        GetTree().Root.AddChild(worldLoader);
        Hide();
        
        Node2D player = ResourceLoader.Load<PackedScene>(playerDirectory).Instantiate<Node2D>();
        scene.AddChild(player);
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
    }
}
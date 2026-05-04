using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class Game : Node {
    public const int BlockSize = 32;
    public World World { get; private set; }

    public event Action Loaded;
    public event Action ExitGameFinished;

    private MultiplayerHost _multiplayerHost;
    private MultiplayerClient _multiplayerClient;

    private Dictionary _playerData;

    public void InitAsSinglePlayer(Dictionary worldData, Dictionary playerData) {
        World = World.CreateAsHost(this, worldData, playerData);
        World.GameLoaded += OnWorldLoaded;
        World.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
        AddChild(World);
        _playerData = FileManager.LoadPlayer(playerData);
    }

    public void InitAsHost(Dictionary worldData, Dictionary playerData) {
        _multiplayerHost = new MultiplayerHost();
        AddChild(_multiplayerHost);

        World = World.CreateAsHost(this, worldData, playerData);
        World.GameLoaded += OnWorldLoaded;
        AddChild(World);
        World.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
        _playerData = FileManager.LoadPlayer(playerData);

       MultiplayerApi.PeerConnectedEventHandler peerConnectedHandler = id => {
            Dictionary metadata = new();
            metadata["Width"] = worldData["Width"];
            metadata["Height"] = worldData["Height"];
            metadata["itemMap"] = World.ItemIdBimap.ToDictionary();
            RpcId(id, nameof(RpcClientCreateWorld), metadata);
        };
        Multiplayer.PeerConnected += peerConnectedHandler;
        TreeExiting += () => {
            Multiplayer.PeerConnected -= peerConnectedHandler;
        };
    }

    public void InitAsClient(Dictionary playerData) {
        _playerData = FileManager.LoadPlayer(playerData);
        _multiplayerClient = new MultiplayerClient();
        AddChild(_multiplayerClient);
    }
    
    [Rpc]
    private void RpcClientCreateWorld(Dictionary metadata) {
        World = World.CreateAsClient(metadata, _playerData, this);
        World.GameLoaded += OnWorldLoaded;
        AddChild(World);
        World.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
    }
    
    private void OnExitGameButtonDown() {
        World.Interface.GameMenu.ExitGameButtonDown -= OnExitGameButtonDown;

        if (World.IsHost) {
            Dictionary worldData = new() {
                ["Name"] = World.WorldData["Name"],
                ["Width"] = World.WorldSize.X,
                ["Height"] = World.WorldSize.Y,
                ["blocks"] = SerializeBlocks(World.BlockManager.Blocks),
                ["walls"] = SerializeBlocks(World.BlockManager.Walls),
                ["props"] = SerializeProps(),
                ["itemMap"] = World.ItemIdBimap.ToDictionary()
            };

            FileManager.SaveWorld(worldData);
        }

        ExitGameFinished?.Invoke();
    }

    private Dictionary<string, Dictionary> SerializeBlocks(Block[,] data) {
        Dictionary<string, Dictionary> groupedByItemId = new();

        for (int x = 0; x < World.WorldSize.X; x++) {
            for (int y = 0; y < World.WorldSize.Y; y++) {
                Block block = data[x, y];
                if (block is null) continue;
                string idStr = block.ItemId.ToString();
                if (!groupedByItemId.ContainsKey(idStr)) {
                    groupedByItemId[idStr] = new Dictionary();
                }

                if (!groupedByItemId[idStr].ContainsKey($"{x}")) {
                    groupedByItemId[idStr][$"{x}"] = new Array();
                }

                ((Array)groupedByItemId[idStr][$"{x}"]).Add(y);
            }
        }

        return groupedByItemId;
    }

    private Dictionary<string, Dictionary> SerializeProps() {
        Dictionary<string, Dictionary> groupedByItemId = new();

        foreach ((Vector2I coords, Prop prop) in World.PropManager.Props) {
            Item item = prop.Item;
            string itemId = World.ItemIdBimap.GetId(item).ToString();

            if (!groupedByItemId.ContainsKey(itemId)) {
                groupedByItemId[itemId] = new Dictionary();
            }

            if (!groupedByItemId[itemId].ContainsKey($"{coords.X}")) {
                groupedByItemId[itemId][$"{coords.X}"] = new Array();
            }

            ((Array)groupedByItemId[itemId][$"{coords.X}"]).Add(coords.Y);
        }

        return groupedByItemId;
    }
    
    private void OnWorldLoaded() {
        World.GameLoaded -= OnWorldLoaded;
        Loaded?.Invoke();
    }
}
using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using TerrariaRipoffNNF.scripts;
using Array = Godot.Collections.Array;

public partial class WorldManager : Node {
    public static WorldManager Instance { get; private set; }

    [Signal]
    public delegate void CreatedServerWorldManagerEventHandler();

    [Signal]
    public delegate void CreatedActiveBlockEventHandler();

    [Export] private PackedScene packedServerData;
    [Export] private PackedScene packedActiveBlock;
    [Export] public int BlockSize { get; private set; } = 40;
    [Export] public int ActiveBlockViewDistance { get; private set; } = 10;

    public ServerData ServerData { get; private set; }

    public override void _Ready() {
        Instance = this;
    }

    private void OnStartedServer() {
        ServerData = packedServerData.Instantiate<ServerData>();
        AddChild(ServerData);
        EmitSignal(SignalName.CreatedServerWorldManager);
    }

    private void OnCreatedLocalPlayer(int xSpawnCoords, int ySpawnCoords) {
        Player.LocalPlayer.LocalPlayerMoved += OnLocalPlayerMoved;
        int peerId = Multiplayer.GetUniqueId();
        RpcId(MultiplayerManager.HOST_ID, nameof(GetSavedBlocksOnServer),
            peerId, xSpawnCoords, ySpawnCoords);
    }

    private void OnLocalPlayerMoved(int xCoords, int yCoords, int prevXCoords, int prevYCoords) {
        int peerId = Multiplayer.GetUniqueId();
        // @todo delete activeBlocks that are out of range
        // RpcId(MultiplayerManager.HOST_ID, nameof(GetSavedBlocksOnServer),
        //     peerId, xCoords, yCoords, prevXCoords, prevYCoords);
    }


    // how to format the savedBlocks data when sending by RPC
    // savedBlock: Resource, check the MustBeVariant tag
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void GetSavedBlocksOnServer(int peerId, int xCoords, int yCoords) {
        int worldWidth = ServerData.WorldWidth;
        int worldHeight = ServerData.WorldHeight;

        int xStart = Math.Max(0, xCoords - ActiveBlockViewDistance);
        int xEnd = Math.Min(worldWidth - 1, xCoords + ActiveBlockViewDistance);
        int yStart = Math.Max(0, yCoords - ActiveBlockViewDistance);
        int yEnd = Math.Min(worldHeight - 1, yCoords + ActiveBlockViewDistance);

        List<(int x, int y)> coordinates = new();

        for (int x = xStart; x < xEnd; x++) {
            for (int y = yStart; y < yEnd; y++) {
                coordinates.Add((x, y));
            }
        }

        Array savedBlocks = new();
        foreach ((int x, int y) in coordinates) {
            var block = ServerData.GetSavedBlock(x, y);
            if (block is not null) {
                savedBlocks.Add(block.Serialize());
            }
        }

        RpcId(peerId, nameof(CreateActiveBlocksOnPeer), savedBlocks);
    }

    [Rpc(CallLocal = true)]
    private void CreateActiveBlocksOnPeer(Array array) {
        // BlockType block = Serializer.Deserialize<BlockType>(y[0]);
        // GD.Print(block.Weight);
        try {
            foreach (Godot.Collections.Dictionary<string, string> dic in array) {
                int xPosition = dic["XPosition"].ToInt();
                int yPosition = dic["YPosition"].ToInt();
                BlockType blockTypeId = (BlockType)InstanceFromId(ulong.Parse(dic["BlockTypeId"]));
                CreateActiveBlock(blockTypeId, xPosition, yPosition);
            }
        }
        catch (Exception e) {
            GD.Print("invalid data");
            GD.Print(e);
        }
    }

    private void CreateActiveBlock(BlockType blockType, int xPosition, int yPosition) {
        ActiveBlock newBlock = packedActiveBlock.Instantiate<ActiveBlock>();
        newBlock.Position = new Vector2(xPosition * BlockSize, yPosition * BlockSize);
        newBlock.BlockType = blockType;
        AddChild(newBlock);
    }
}
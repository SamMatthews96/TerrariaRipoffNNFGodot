using System;
using System.Collections.Generic;
using Godot;
using Array = Godot.Collections.Array;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Scenes.Scripts;

public partial class WorldManager : Node {
    public static WorldManager Instance { get; private set; }

    [Signal]
    public delegate void CreatedServerWorldManagerEventHandler();

    [Export] private PackedScene packedServerData;
    [Export] public int BlockSize { get; private set; } = 40;
    [Export] public int ActiveBlockViewDistance { get; private set; } = 10;

    public ServerData ServerData { get; private set; }

    private int worldWidth;
    private int worldHeight;
    private ActiveBlock[,] activeBlocks;

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
        RpcId(MultiplayerManager.HOST_ID, nameof(ServerSendSavedBlocksOnSpawn),
            peerId, xSpawnCoords, ySpawnCoords);
    }

    private void OnLocalPlayerMoved(
        int newXCoordinate, int newYCoordinate, int oldXCoordinate, int oldYCoordinate) {
        int peerId = Multiplayer.GetUniqueId();
        RpcId(MultiplayerManager.HOST_ID, nameof(ServerSendSavedBlocksOnMove),
            peerId, newXCoordinate, newYCoordinate, oldXCoordinate, oldYCoordinate);
        DeleteActiveBlocksInRegion(newXCoordinate, newYCoordinate, oldXCoordinate, oldYCoordinate);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerSendSavedBlocksOnSpawn(int peerId, int xCoordinate, int yCoordinate) {
        worldWidth = ServerData.WorldWidth;
        worldHeight = ServerData.WorldHeight;
        
        (int left, int right, int top, int bottom) = GetRegionBoundary(xCoordinate, yCoordinate);

        Array savedBlocksSerialized = new();
        for (int x = left; x < right; x++) {
            for (int y = top; y < bottom; y++) {
                var block = ServerData.GetSavedBlock(x, y);
                if (block is not null) {
                    savedBlocksSerialized.Add(block.Serialize());
                }
            }
        }

        RpcId(peerId, nameof(PeerCreateWorld),
            ServerData.WorldWidth, ServerData.WorldHeight, savedBlocksSerialized);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerSendSavedBlocksOnMove(
        int peerId, int newXCoordinate, int newYCoordinate, int lastXCoordinate, int lastYCoordinate) {
        (int newLeft, int newRight, int newTop, int newBottom) =
            GetRegionBoundary(newXCoordinate, newYCoordinate);

        Array savedBlocksSerialized = new();
        for (int x = newLeft; x < newRight; x++) {
            for (int y = newTop; y < newBottom; y++) {
                if (Math.Abs(lastXCoordinate - x) < ActiveBlockViewDistance &&
                    Math.Abs(lastYCoordinate - y) < ActiveBlockViewDistance)
                    continue;

                var block = ServerData.GetSavedBlock(x, y);
                if (block is not null) {
                    savedBlocksSerialized.Add(block.Serialize());
                }
            }
        }

        RpcId(peerId, nameof(PeerCreateActiveBlocks), savedBlocksSerialized);
    }

    private (int xStart, int xEnd, int yStart, int yEnd) GetRegionBoundary(int xCoord, int yCoord) {
        int xStart = Math.Max(0, xCoord - ActiveBlockViewDistance);
        int xEnd = Math.Min(worldWidth - 1, xCoord + ActiveBlockViewDistance);
        int yStart = Math.Max(0, yCoord - ActiveBlockViewDistance);
        int yEnd = Math.Min(worldHeight - 1, yCoord + ActiveBlockViewDistance);
        return (xStart, xEnd, yStart, yEnd);
    }

    [Rpc(CallLocal = true)]
    private void PeerCreateWorld(int serverWorldWidth, int serverWorldHeight, Array savedBlocksSerialized) {
        worldWidth = serverWorldWidth;
        worldHeight = serverWorldHeight;
        activeBlocks = new ActiveBlock[serverWorldWidth, serverWorldHeight];
        PeerCreateActiveBlocks(savedBlocksSerialized);
    }

    [Rpc(CallLocal = true)]
    private void PeerCreateActiveBlocks(Array savedBlocksSerialized) {
        try {
            foreach (Godot.Collections.Dictionary<string, string> blockSerialized in savedBlocksSerialized) {
                int xPosition = blockSerialized["XPosition"].ToInt();
                int yPosition = blockSerialized["YPosition"].ToInt();
                BlockType blockTypeId = (BlockType)InstanceFromId(ulong.Parse(blockSerialized["BlockTypeId"]));
                CreateActiveBlock(blockTypeId, xPosition, yPosition);
            }
        }
        catch (Exception e) {
            GD.Print("invalid data");
            GD.Print(e);
        }
    }

    private void CreateActiveBlock(BlockType blockType, int xPosition, int yPosition) {
        if (activeBlocks[xPosition, yPosition] is not null) return;
        
        Vector2 position = new Vector2(xPosition * BlockSize, yPosition * BlockSize);
        ActiveBlock newBlock = ActiveBlock.Instantiate(blockType, position);
        activeBlocks[xPosition, yPosition] = newBlock;
        AddChild(newBlock);
    }

    private void DeleteActiveBlocksInRegion(
        int newXCoordinate, int newYCoordinate, int oldXCoordinate, int oldYCoordinate) {
        (int oldLeft, int oldRight, int oldTop, int oldBottom) =
            GetRegionBoundary(oldXCoordinate, oldYCoordinate);
        
        for (int x = oldLeft; x < oldRight; x++) {
            for (int y = oldTop; y < oldBottom; y++) {
                if (Math.Abs(newXCoordinate - x) < ActiveBlockViewDistance &&
                    Math.Abs(newYCoordinate - y) < ActiveBlockViewDistance)
                    continue;
                ActiveBlock activeBlock = activeBlocks[x, y];
                if (activeBlock is null) continue;
                
                activeBlock.QueueFree();
                activeBlocks[x, y] = null;
            }
        }
    }
}
using System;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Scenes.Scripts;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class WorldManager : Node {
    public const int BLOCK_SIZE = 32;
    private const int BLOCK_RENDER_DISTANCE = 10;

    [Export] private PackedScene packedServerData;
    private int worldWidth;
    private int worldHeight;
    private int spawnX;
    private int spawnY;
    private ActiveBlock[,] activeBlocks;
    private SavedBlock[,] savedBlocks;

    [Signal]
    public delegate void CreatedWorldManagerEventHandler();

    private void OnStartedServer() {
        // PH
        // load world data from disk
        int spawnX = 5;
        int spawnY = 5;
        worldWidth = 50;
        worldHeight = 50;
        
        BlockType blockType = ResourceLoader.Load<BlockType>("res://Resources/BlockType/Stone.tres");
        savedBlocks = new SavedBlock[worldWidth, worldHeight];
        for (int x = 0; x < worldWidth; x++) {
            savedBlocks[x, 6] = new SavedBlock(blockType, x, 6);
        }
        // END PH

        GetAndCreateBlocks(1, spawnX, spawnY);
    }
    
    private void OnLocalPlayerCreated(int xSpawnCoords, int ySpawnCoords) {
        // Player.LocalPlayer.LocalPlayerMoved += OnLocalPlayerMoved;
        int peerId = Multiplayer.GetUniqueId();
        RpcId(MultiplayerManager.HOST_ID, nameof(GetAndCreateBlocks),
            peerId, xSpawnCoords, ySpawnCoords);
    }

    private void OnLocalPlayerMoved(
        int newXCoordinate, int newYCoordinate, int oldXCoordinate, int oldYCoordinate) {
        int peerId = Multiplayer.GetUniqueId();
        // RpcId(MultiplayerManager.HOST_ID, nameof(GetAndCreateNewBlocks),
        //     peerId, newXCoordinate, newYCoordinate, oldXCoordinate, oldYCoordinate);
        // DeleteActiveBlocksInRegion(newXCoordinate, newYCoordinate, oldXCoordinate, oldYCoordinate);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void GetAndCreateBlocks(int peerId, int xCoord, int yCoord) {
        (int xStart, int xEnd, int yStart, int yEnd) = GetRegionBoundary(xCoord, yCoord);

        Array savedBlocksSerialized = new();
        for (int x = xStart; x < xEnd; x++) {
            for (int y = yStart; y < yEnd; y++) {
                var block = savedBlocks[x, y];
                if (block is not null) {
                    savedBlocksSerialized.Add(block.Serialize());
                }
            }
        }

        RpcId(peerId, nameof(PeerCreateWorld), worldWidth, worldHeight, savedBlocksSerialized);
    }

    [Rpc(CallLocal = true)]
    public void PeerCreateWorld(int serverWorldWidth, int serverWorldHeight, Array savedBlocksSerialized) {
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

        Vector2 position = new Vector2(xPosition * BLOCK_SIZE, yPosition * BLOCK_SIZE);
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
                if (Math.Abs(newXCoordinate - x) < BLOCK_RENDER_DISTANCE &&
                    Math.Abs(newYCoordinate - y) < BLOCK_RENDER_DISTANCE)
                    continue;
                ActiveBlock activeBlock = activeBlocks[x, y];
                if (activeBlock is null) continue;

                activeBlock.QueueFree();
                activeBlocks[x, y] = null;
            }
        }
    }

    private (int xStart, int xEnd, int yStart, int yEnd) GetRegionBoundary(int xCoord, int yCoord) {
        int xStart = Math.Max(0, xCoord - BLOCK_RENDER_DISTANCE);
        int xEnd = Math.Min(worldWidth - 1, xCoord + BLOCK_RENDER_DISTANCE);
        int yStart = Math.Max(0, yCoord - BLOCK_RENDER_DISTANCE);
        int yEnd = Math.Min(worldHeight - 1, yCoord + BLOCK_RENDER_DISTANCE);
        return (xStart, xEnd, yStart, yEnd);
    }
}
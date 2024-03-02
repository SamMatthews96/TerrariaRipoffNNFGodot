using System;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Scenes.Scripts;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class WorldManager : Node {
    public const int BLOCK_RENDER_DISTANCE = 10;
    public const int BLOCK_SIZE = 32;

    [Export] private PackedScene packedServerData;
    private int worldWidth;
    private int worldHeight;
    private ActiveBlock[,] activeBlocks;

    [Signal]
    public delegate void CreatedServerWorldManagerEventHandler();

    public static WorldManager Instance { get; private set; }

    public override void _Ready() {
        Instance = this;
    }
    
    // private void OnLocalPlayerCreated(int xSpawnCoords, int ySpawnCoords) {
    //     // Player.LocalPlayer.LocalPlayerMoved += OnLocalPlayerMoved;
    //     int peerId = Multiplayer.GetUniqueId();
    //     RpcId(MultiplayerManager.HOST_ID, nameof(ServerData.Instance.GetAndCreateBlocks),
    //         peerId, xSpawnCoords, ySpawnCoords);
    // }

    private void OnLocalPlayerMoved(
        int newXCoordinate, int newYCoordinate, int oldXCoordinate, int oldYCoordinate) {
        int peerId = Multiplayer.GetUniqueId();
        RpcId(MultiplayerManager.HOST_ID, nameof(ServerData.Instance.GetAndCreateNewBlocks),
            peerId, newXCoordinate, newYCoordinate, oldXCoordinate, oldYCoordinate);
        DeleteActiveBlocksInRegion(newXCoordinate, newYCoordinate, oldXCoordinate, oldYCoordinate);
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
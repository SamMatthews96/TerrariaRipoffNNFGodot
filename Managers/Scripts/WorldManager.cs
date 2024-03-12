using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Scenes.Scripts;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class WorldManager : Node {
    public const int BLOCK_SIZE = 32;
    private const int BLOCK_RENDER_DISTANCE = 20;

    private int worldWidth;
    private int worldHeight;
    private int spawnX;
    private int spawnY;
    private ActiveBlock[,] activeBlocks;
    private SavedBlock[,] savedBlocks;

    [Signal]
    public delegate void WorldCreatedEventHandler(int spawnX, int spawnY);

    private void OnWorldLoaded(World world) {
        spawnX = 5;
        spawnY = 5;
        worldWidth = world.WorldWidth;
        worldHeight = world.WorldHeight;

        savedBlocks = world.SavedBlocks;
        foreach (SavedBlock savedBlock in savedBlocks) {
            if (savedBlock is null) continue;
            savedBlock.Destroyed += OnSavedBlockDestroyed;
        }

        GetAndCreateBlocksOnSpawn(1, spawnX, spawnY);
    }

    private void OnConnectedToServer() {
        spawnX = 10;
        spawnY = 5;

        int peerId = Multiplayer.GetUniqueId();
        RpcId(1, nameof(GetAndCreateBlocksOnSpawn),
            peerId, spawnX, spawnY);
    }

    private void OnCreatedLocalPlayerOnServer(Vector2 _) {
        Player.LocalPlayer.LocalPlayerMoved += OnLocalPlayerMoved;
        Player.LocalPlayer.LocalPlayerClicked += OnPlayerAttemptBuildBlock;
    }

    private void OnLocalPlayerMoved(
        int newXCoordinate, int newYCoordinate, int oldXCoordinate, int oldYCoordinate) {
        int peerId = Multiplayer.GetUniqueId();
        RpcId(MultiplayerManager.HOST_ID, nameof(GetAndCreateBlocksOnMoved),
            peerId, newXCoordinate, newYCoordinate, oldXCoordinate, oldYCoordinate);
        DeleteActiveBlocksInRegion(newXCoordinate, newYCoordinate, oldXCoordinate, oldYCoordinate);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void GetAndCreateBlocksOnSpawn(int peerId, int xCoord, int yCoord) {
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

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void GetAndCreateBlocksOnMoved(
        int peerId, int xCoord, int yCoord, int oldXCoord, int oldYCoord) {
        (int xStart, int xEnd, int yStart, int yEnd) = GetRegionBoundary(xCoord, yCoord);

        Array savedBlocksSerialized = new();
        for (int x = xStart; x <= xEnd; x++) {
            for (int y = yStart; y <= yEnd; y++) {
                if (Math.Abs(oldXCoord - x) < BLOCK_RENDER_DISTANCE &&
                    Math.Abs(oldYCoord - y) < BLOCK_RENDER_DISTANCE)
                    continue;

                var block = savedBlocks[x, y];
                if (block is not null) {
                    savedBlocksSerialized.Add(block.Serialize());
                }
            }
        }

        RpcId(peerId, nameof(PeerCreateActiveBlocks), savedBlocksSerialized);
    }

    [Rpc(CallLocal = true)]
    public void PeerCreateWorld(int serverWorldWidth, int serverWorldHeight, Array savedBlocksSerialized) {
        worldWidth = serverWorldWidth;
        worldHeight = serverWorldHeight;
        activeBlocks = new ActiveBlock[serverWorldWidth, serverWorldHeight];
        PeerCreateActiveBlocks(savedBlocksSerialized);
        EmitSignal(SignalName.WorldCreated, spawnX, spawnY);
    }

    [Rpc(CallLocal = true)]
    private void PeerCreateActiveBlocks(Array savedBlocksSerialized) {
        try {
            foreach (Dictionary<string, string> blockSerialized in savedBlocksSerialized) {
                int xPosition = blockSerialized["XPosition"].ToInt();
                int yPosition = blockSerialized["YPosition"].ToInt();
                BlockType blockType = ResourceLoader.Load<BlockType>(blockSerialized["ResourcePath"]);

                if (activeBlocks[xPosition, yPosition] is not null) continue;
                ActiveBlock newBlock = ActiveBlock.Instantiate(blockType, xPosition, yPosition);
                newBlock.TakenDamage += OnActiveBlockTakenDamage;
                activeBlocks[xPosition, yPosition] = newBlock;
                AddChild(newBlock);
            }
        }
        catch (Exception e) {
            GD.Print("invalid data");
            GD.Print(e);
        }
    }

    private void DeleteActiveBlocksInRegion(
        int newXCoordinate, int newYCoordinate, int oldXCoordinate, int oldYCoordinate) {
        (int oldLeft, int oldRight, int oldTop, int oldBottom) =
            GetRegionBoundary(oldXCoordinate, oldYCoordinate);

        for (int x = oldLeft; x <= oldRight; x++) {
            for (int y = oldTop; y <= oldBottom; y++) {
                if (Math.Abs(newXCoordinate - x) < BLOCK_RENDER_DISTANCE &&
                    Math.Abs(newYCoordinate - y) < BLOCK_RENDER_DISTANCE)
                    continue;
                if (activeBlocks[x, y] is null) continue;
                activeBlocks[x, y].QueueFree();
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

    private void OnActiveBlockTakenDamage(int xPosition, int yPosition, float damageAmount) {
        RpcId(MultiplayerManager.HOST_ID, nameof(DamageSavedBlock),
            xPosition, yPosition, damageAmount);
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void DamageSavedBlock(int xPosition, int yPosition, float damageAmount) {
        SavedBlock savedBlock = savedBlocks[xPosition, yPosition];
        savedBlock?.TakeDamage(damageAmount);
    }

    private void OnSavedBlockDestroyed(int xPosition, int yPosition) {
        savedBlocks[xPosition, yPosition] = null;
        Rpc(nameof(DeleteActiveBlock), xPosition, yPosition);
    }

    [Rpc(CallLocal = true)]
    private void DeleteActiveBlock(int xPosition, int yPosition) {
        if (activeBlocks[xPosition, yPosition] is null) return;
        activeBlocks[xPosition, yPosition].QueueFree();
        activeBlocks[xPosition, yPosition] = null;
    }

    private void OnPlayerAttemptBuildBlock(int xPosition, int yPosition, BlockType blockType) {
        RpcId(MultiplayerManager.HOST_ID, nameof(ServerAttemptBuildBlock),
            xPosition, yPosition, blockType);
    }

    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerAttemptBuildBlock(int xPosition, int yPosition, BlockType blockType) {
        if (savedBlocks[xPosition, yPosition] is not null) return;
        SavedBlock savedBlock = new SavedBlock(blockType, xPosition, yPosition);
        savedBlocks[xPosition, yPosition] = savedBlock;
        Dictionary serializedSavedBlock = savedBlock.Serialize();
        Rpc(nameof(CreateNewSavedBlockAsActive), serializedSavedBlock);
    }

    [Rpc(CallLocal = true)]
    private void CreateNewSavedBlockAsActive(Dictionary<string,string> blockSerialized) {
        int xPosition = blockSerialized["XPosition"].ToInt();
        int yPosition = blockSerialized["YPosition"].ToInt();
        BlockType blockType = ResourceLoader.Load<BlockType>(blockSerialized["ResourcePath"]);
        
        ActiveBlock newBlock = ActiveBlock.Instantiate(blockType, xPosition, yPosition);
        newBlock.TakenDamage += OnActiveBlockTakenDamage;
        activeBlocks[xPosition, yPosition] = newBlock;
        AddChild(newBlock);
    }
}
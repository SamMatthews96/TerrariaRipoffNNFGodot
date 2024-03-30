using System;
using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Utils;
using GodotDictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Resources.Scripts;

public partial class World : Resource {
    public string Name { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public SavedBlock[,] SavedBlocks { get; private set; }
    public Dictionary<string, GridPosition> PlayerPositions { get; } = new();

    public GridPosition DefaultSpawnPosition { get; set; } = new(5, 5);

    public World GetInitialWorldAroundSpawn(int xSpawnCoordinate, int ySpawnCoordinate) {
        SavedBlock[,] savedBlocks = new SavedBlock[Width, Height];
        (int xStart, int xEnd, int yStart, int yEnd) = GetRegionBoundary(xSpawnCoordinate, ySpawnCoordinate);
        for (int x = xStart; x < xEnd; x++) {
            for (int y = yStart; y < yEnd; y++) {
                savedBlocks[x, y] = SavedBlocks[x, y];
            }
        }

        return Builder.New(Name, Width, Height)
            .WithSavedBlocks(savedBlocks)
            .Build();
    }

    public (int xStart, int xEnd, int yStart, int yEnd) GetRegionBoundary(int xCoord, int yCoord) {
        int xStart = Math.Max(0, xCoord - LocalObjectSpawnManager.BLOCK_RENDER_DISTANCE);
        int xEnd = Math.Min(Width - 1, xCoord + LocalObjectSpawnManager.BLOCK_RENDER_DISTANCE);
        int yStart = Math.Max(0, yCoord - LocalObjectSpawnManager.BLOCK_RENDER_DISTANCE);
        int yEnd = Math.Min(Height - 1, yCoord + LocalObjectSpawnManager.BLOCK_RENDER_DISTANCE);
        return (xStart, xEnd, yStart, yEnd);
    }

    public bool AreCoordsInBounds(int xPosition, int yPosition) {
        if (xPosition < 0) return false;
        if (yPosition < 0) return false;
        if (xPosition >= Width) return false;
        if (yPosition >= Height) return false;
        return true;
    }

    public WorldBasicInfo GetBasicInfo() {
        return new WorldBasicInfo(Name, Width, Height);
    }

    public GodotDictionary Serialize() {
        GodotDictionary serializedData = new();
        serializedData.Add("Name", Name);
        serializedData.Add("Width", Width);
        serializedData.Add("Height", Height);

        Array playerPositionsSerialized = new();
        foreach (KeyValuePair<string, GridPosition> playerPosition in PlayerPositions) {
            GodotDictionary playerPositionSerialized = new();
            playerPositionSerialized.Add("X", playerPosition.Value.X);
            playerPositionSerialized.Add("Y", playerPosition.Value.Y);
            playerPositionsSerialized.Add(playerPositionSerialized);
        }

        serializedData.Add("PlayerPositions", playerPositionsSerialized);

        Array savedBlocksSerialized = new();
        foreach (SavedBlock block in SavedBlocks) {
            if (block is null) continue;
            savedBlocksSerialized.Add(block.Serialize());
        }

        serializedData.Add("SavedBlocks", savedBlocksSerialized);

        return serializedData;
    }

    public static World FromDict(GodotDictionary dictionary) {
        try {
            int worldWidth = dictionary["Width"].ToString().ToInt();
            int worldHeight = dictionary["Height"].ToString().ToInt();
            string worldName = dictionary["Name"].ToString();

            Array savedBlocksArray = dictionary["SavedBlocks"].AsGodotArray();
            SavedBlock[,] savedBlocks = new SavedBlock[worldWidth, worldHeight];
            foreach (GodotDictionary savedBlockDict in savedBlocksArray) {
                SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDict);
                if (savedBlock is null) continue;
                savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = savedBlock;
            }

            World world = Builder.New(worldName, worldWidth, worldHeight)
                .WithSavedBlocks(savedBlocks)
                .Build();

            Array playerPositionsArray = dictionary["PlayerPositions"].AsGodotArray();
            foreach (GodotDictionary playerPositionDict in playerPositionsArray) {
                string uniqueName = playerPositionDict["UniqueName"].ToString();
                int xPosition = playerPositionDict["X"].ToString().ToInt();
                int yPosition = playerPositionDict["Y"].ToString().ToInt();
                world.PlayerPositions.Add(uniqueName, new GridPosition(xPosition, yPosition));
            }

            return world;
        }
        catch (Exception e) {
            GD.Print("error reading _world from dict");
            GD.Print(e);
            throw new NotImplementedException();
        }
    }
}
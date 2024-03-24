using System;
using System.Collections.Generic;
using Godot;
using GodotDictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Resources.Scripts;

public partial class World : Resource {
    public string Name { get; private set; }
    public int WorldWidth { get; private set; }
    public int WorldHeight { get; private set; }
    public SavedBlock[,] SavedBlocks { get; }
    public Dictionary<string,PlayerPosition> PlayerPositions { get; } = new();
    
    public int DefaultSpawnX = 5;
    public int DefaultSpawnY = 5;
    
    public class PlayerPosition {
        public int XPosition { get; set; }
        public int YPosition { get; set; }

        public PlayerPosition(int xPosition, int yPosition) {
            XPosition = xPosition;
            YPosition = yPosition;
        }
    }

    public World(SavedBlock[,] savedBlocks, string name, int worldWidth, int worldHeight) {
        Name = name;
        WorldWidth = worldWidth;
        WorldHeight = worldHeight;
        SavedBlocks = savedBlocks;
    }

    public WorldBasicInfo GetBasicInfo() {
        return new WorldBasicInfo(Name, WorldWidth, WorldHeight);
    }

    public GodotDictionary Serialize() {
        GodotDictionary serializedData = new();
        serializedData.Add("Name", Name);
        serializedData.Add("WorldWidth", WorldWidth);
        serializedData.Add("WorldHeight", WorldHeight);
        
        Array playerPositionsSerialized = new();
        foreach (KeyValuePair<string, PlayerPosition> playerPosition in PlayerPositions) {
            GodotDictionary playerPositionSerialized = new();
            playerPositionSerialized.Add("XPosition", playerPosition.Value.XPosition);
            playerPositionSerialized.Add("YPosition", playerPosition.Value.YPosition);
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
            int worldWidth = dictionary["WorldWidth"].ToString().ToInt();
            int worldHeight = dictionary["WorldHeight"].ToString().ToInt();
            string worldName = dictionary["Name"].ToString();

            Array savedBlocksArray = dictionary["SavedBlocks"].AsGodotArray();
            SavedBlock[,] savedBlocks = new SavedBlock[worldWidth, worldHeight];
            foreach (GodotDictionary savedBlockDict in savedBlocksArray) {
                SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDict);
                savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = savedBlock;
            }
            
            World world = new(
                savedBlocks,
                worldName,
                worldWidth,
                worldHeight);
            
            Array playerPositionsArray = dictionary["PlayerPositions"].AsGodotArray();
            foreach (GodotDictionary playerPositionDict in playerPositionsArray) {
                string uniqueName = playerPositionDict["UniqueName"].ToString();
                int xPosition = playerPositionDict["XPosition"].ToString().ToInt();
                int yPosition = playerPositionDict["YPosition"].ToString().ToInt();
                world.PlayerPositions.Add(uniqueName, new PlayerPosition(xPosition, yPosition));
            }
            return world;
        }
        catch (Exception e) {
            GD.Print("error reading World from dict");
            GD.Print(e);
            throw new NotImplementedException();
        }
    }
}
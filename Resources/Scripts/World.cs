using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Resources.Scripts;

public partial class World : Resource {
    public string Name { get; private set; }
    public int WorldWidth { get; private set; }
    public int WorldHeight { get; private set; }
    public SavedBlock[,] SavedBlocks { get; }

    public World(SavedBlock[,] savedBlocks, string name, int worldWidth, int worldHeight) {
        Name = name;
        WorldWidth = worldWidth;
        WorldHeight = worldHeight;
        SavedBlocks = savedBlocks;
    }

    public WorldBasicInfo GetBasicInfo() {
        return new WorldBasicInfo(Name, WorldWidth, WorldHeight);
    }

    public Dictionary Serialize() {
        Dictionary serializedData = new();
        serializedData.Add("Name", Name);
        serializedData.Add("WorldWidth", WorldWidth);
        serializedData.Add("WorldHeight", WorldHeight);
        Array savedBlocksSerialized = new();
        foreach (SavedBlock block in SavedBlocks) {
            if (block is null) continue;
            savedBlocksSerialized.Add(block.Serialize());
        }

        serializedData.Add("SavedBlocks", savedBlocksSerialized);
        return serializedData;
    }

    public static World FromDict(Dictionary dictionary) {
        try {
            int worldWidth = dictionary["WorldWidth"].ToString().ToInt();
            int worldHeight = dictionary["WorldHeight"].ToString().ToInt();
            string worldName = dictionary["Name"].ToString();

            Array savedBlocksArray = dictionary["SavedBlocks"].AsGodotArray();
            SavedBlock[,] savedBlocks = new SavedBlock[worldWidth, worldHeight];
            foreach (Dictionary savedBlockDict in savedBlocksArray) {
                SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDict);
                savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = savedBlock;
            }
            return new World(
                savedBlocks,
                worldName,
                worldWidth,
                worldHeight);
        }
        catch (Exception e) {
            GD.Print("error reading World from dict");
            GD.Print(e);
            throw new NotImplementedException();
        }
    }
}
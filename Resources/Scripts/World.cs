using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.scripts;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Resources.Scripts;

public partial class World : WorldBasicInfo, ISerializable {
    public SavedBlock[,] SavedBlocks { get; }

    public World(SavedBlock[,] savedBlocks, string name, int worldWidth, int worldHeight)
        : base(name, worldWidth, worldHeight) {
        SavedBlocks = savedBlocks;
    }

    public WorldBasicInfo GetBasicInfo() {
        return new WorldBasicInfo(Name, WorldWidth, WorldHeight);
    }

    public new Dictionary Serialize() {
        Dictionary serializedData = base.Serialize();
        Array savedBlocksSerialized = new();
        foreach (var block in SavedBlocks) {
            if (block is null) continue;
            savedBlocksSerialized.Add(block.Serialize());
        }

        serializedData.Add("SavedBlocks", savedBlocksSerialized);
        return serializedData;
    }

    public new static World FromDict(Dictionary dictionary) {
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
            GD.Print("invalid World dict");
            GD.Print(e);
            throw new NotImplementedException();
        }
    }
}
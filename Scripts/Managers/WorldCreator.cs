using System;
using Godot.Collections;
using TerrariaRipoffNNF.TestScenes;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public static class WorldCreator {
    public static void CreateWorld(WorldBasicInfo worldBasicInfo) {
        Dictionary worldDictionary = new();
        worldDictionary.Add("Name", worldBasicInfo.Name);
        worldDictionary.Add("Width", worldBasicInfo.Width);
        worldDictionary.Add("Height", worldBasicInfo.Height);

        worldDictionary.Add("PlayerPositions", new Array());
        worldDictionary.Add("DefaultSpawnPosition", new Array { 5, 5 });

        int mid = 15;

        Item[] items = {
            Data.Items.Stone,
            Data.Items.Earth,
            Data.Items.IronOre
        };
        Random random = new();

        ItemIdBimap map = new();
        
        int earthId = map.GetId(Data.Items.Earth);
        
        Array blockList = new();
        Array wallList = new();
        for (int x = 0; x < worldBasicInfo.Width; x++) {
            for (int y = mid; y < worldBasicInfo.Height; y++) {
                Item item = items[random.Next(items.Length)];
                Dictionary newBlock = new() {
                    // get id from item
                    { "item", map.GetId(item) },
                    { "xPosition", x },
                    { "yPosition", y }
                };
                blockList.Add(newBlock);
                
                Dictionary newWall = new() {
                    { "item", earthId },
                    { "xPosition", x },
                    { "yPosition", y }
                };
                wallList.Add(newWall);
            }
        }

        worldDictionary.Add("blocks", blockList);
        worldDictionary.Add("walls", wallList);
        worldDictionary.Add("itemMap", map.ToDictionary());

        FileManager.SaveWorld(worldDictionary);
    }
}
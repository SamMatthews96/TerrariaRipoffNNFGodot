using System;
using Godot.Collections;
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

        Array blockList = new();
        for (int x = 0; x < worldBasicInfo.Width; x++) {
            for (int y = mid; y < worldBasicInfo.Height; y++) {
                Item item = items[random.Next(items.Length)];
                Dictionary newBlock = new() {
                    { "item", item.ToDictionary() },
                    { "xPosition", x },
                    { "yPosition", y },
                };
                blockList.Add(newBlock);
            }
        }

        worldDictionary.Add("blocks", blockList);

        FileManager.SaveWorld(worldDictionary);
    }
}
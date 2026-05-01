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

        int mid = 15;

        Item[] items = {
            Data.Items.Earth,
            Data.Items.Stone,
            Data.Items.IronOre
        };

        Array<string> idToString = new();
        Dictionary<string, ushort> stringToId = new();
        Dictionary<string, Dictionary> stringToItem = new();
        int count = 0;
        foreach (Item item in items) {
            string itemCode = item.ResourcePath;
            Dictionary itemDict = item.ToDictionary();
            idToString.Add(itemCode);
            stringToId.Add(itemCode, (ushort)count);
            stringToItem.Add(itemCode, itemDict);
            count++;
        }

        Dictionary mapDict = new() {
            { "IdToString", idToString },
            { "StringToId", stringToId },
            { "StringToItem", stringToItem },
        };

        int earthId = 0;
        Random random = new();
        Array blockList = new();
        Array wallList = new();
        for (int x = 0; x < worldBasicInfo.Width; x++) {
            for (int y = mid; y < worldBasicInfo.Height; y++) {
                int itemId = random.Next(0, 3);
                Dictionary newBlock = new() {
                    // get id from item
                    { "id", itemId },
                    { "x", x },
                    { "y", y }
                };
                blockList.Add(newBlock);

                Dictionary newWall = new() {
                    { "id", earthId },
                    { "x", x },
                    { "y", y }
                };
                wallList.Add(newWall);
            }
        }

        Array<Dictionary> breakables = new();
        for (int x = 10; x < worldBasicInfo.Width - 10; x += 10) {
            for (int y = 5; y < mid; y++) {
                Dictionary tree = new() {
                    { "x", x },
                    { "y", y },
                    { "id", 1 }
                };
                breakables.Add(tree);
            }
        }

        worldDictionary.Add("breakables", breakables);
        worldDictionary.Add("blocks", blockList);
        worldDictionary.Add("walls", wallList);
        worldDictionary.Add("itemMap", mapDict);

        FileManager.SaveWorld(worldDictionary);
    }
}
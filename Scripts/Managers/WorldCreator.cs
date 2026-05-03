using System;
using Godot.Collections;
using TerrariaRipoffNNF.TestScenes;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public static class WorldCreator {
    public static void CreateWorld(WorldBasicInfo worldBasicInfo) {
        int mid = 15;

        Item[] items = {
            Data.Items.Earth,
            Data.Items.Stone,
            Data.Items.IronOre
        };

        int earthId = 0;
        Dictionary<string, Array> blockList = new();
        Dictionary<string, Array> wallList = new() {
            { $"{earthId}", new Array() }
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
            blockList.Add($"{count}", new Array());
            count++;
        }

        Dictionary mapDict = new() {
            { "IdToString", idToString },
            { "StringToId", stringToId },
            { "StringToItem", stringToItem },
        };

        Random random = new();
        for (int x = 0; x < worldBasicInfo.Width; x++) {
            for (int y = mid; y < worldBasicInfo.Height; y++) {
                int itemId = random.Next(0, 3);
                blockList[$"{itemId}"].Add(new Array { x, y });
                wallList[$"{earthId}"].Add(new Array { x, y });
            }
        }

        int treeId = 1;
        Dictionary<string, Array> propList = new() { { $"{treeId}", new Array() } };
        for (int x = 10; x < worldBasicInfo.Width - 10; x += 10) {
            for (int y = 5; y < mid; y++) {
                propList[$"{treeId}"].Add(new Array { x, y });
            }
        }

        Dictionary worldDictionary = new();
        worldDictionary.Add("Name", worldBasicInfo.Name);
        worldDictionary.Add("Width", worldBasicInfo.Width);
        worldDictionary.Add("Height", worldBasicInfo.Height);
        worldDictionary.Add("props", propList);
        worldDictionary.Add("blocks", blockList);
        worldDictionary.Add("walls", wallList);
        worldDictionary.Add("itemMap", mapDict);

        FileManager.SaveWorld(worldDictionary);
    }
}
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
            Data.Items.IronOre,
            Data.Items.Tree,
        };

        int earthId = 0;
        Dictionary<string, Dictionary> blockList = new();
        Dictionary<string, Dictionary> wallList = new();

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
            blockList.Add($"{count}", new Dictionary());
            wallList.Add($"{count}", new Dictionary());
            count++;
        }

        Dictionary mapDict = new() {
            { "IdToString", idToString },
            { "StringToId", stringToId },
            { "StringToItem", stringToItem },
        };

        Random random = new();
        for (int x = 0; x < worldBasicInfo.Width; x++) {
            for (int itemId = 0; itemId < items.Length; itemId++) {
                blockList[$"{itemId}"].Add($"{x}", new Array());
                wallList[$"{itemId}"].Add($"{x}", new Array());
            }
            for (int y = mid; y < worldBasicInfo.Height; y++) {
                int itemId = random.Next(0, 3);
                
                ((Array)blockList[$"{itemId}"][$"{x}"]).Add(y);
                ((Array)wallList[$"{earthId}"][$"{x}"]).Add(y);
            }
        }

        int treeId = 3;
        Dictionary<string, Dictionary> propList = new() { { $"{treeId}", new Dictionary() } };
        for (int x = 10; x < worldBasicInfo.Width - 10; x += 10) {
            propList[$"{treeId}"][$"{x}"] = new Array();
            for (int y = 5; y < mid; y++) {
                ((Array)propList[$"{treeId}"][$"{x}"]).Add(y);
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
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
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
        };
        Random random = new();

        Array savedWorldObjects = new();
        for (int x = 0; x < worldBasicInfo.Width; x++) {
            for (int y = mid; y < worldBasicInfo.Height; y++) {
                Item item = items[random.Next(items.Length)];
                Dictionary newBlock = new() {
                    { "type", "block" },
                    { "item", item.ToDictionary() },
                    { "xPosition", x },
                    { "yPosition", y },
                };
                savedWorldObjects.Add(newBlock);
            }
        }

        Item wood = Data.Items.Wood;

        for (int y = mid - 10; y < mid - 1; y++) {
            Dictionary tree = new() {
                { "type", "tree" },
                { "item", wood.ToDictionary() },
                { "xPosition", 10 },
                { "yPosition", y },
            };
            savedWorldObjects.Add(tree);
        }

        worldDictionary.Add("SavedWorldObjects", savedWorldObjects);

        FileManager.SaveWorld(worldDictionary);
    }
}
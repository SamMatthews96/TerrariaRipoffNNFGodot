using System;
using System.Collections.Generic;
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
        Item[] types = {
            Data.Items.Stone,
            Data.Items.Earth,
            Data.Items.FerriumOre
        };
        Random random = new();

        Array savedWorldObjects = new();
        for (int x = 0; x < worldBasicInfo.Width; x++) {
            for (int y = mid; y < worldBasicInfo.Height; y++) {
                Item item = types[random.Next(3)];
                Dictionary newBlock = new() {
                    {"type", "block"},
                    {"item", item.ToDictionary()},
                    {"xPosition", x},
                    {"yPosition", y},
                };
                savedWorldObjects.Add(newBlock);
            }
        }

        for (int y = 0; y < 10; y++) {
            Dictionary newProp = new() {
                {"type", "prop"},
                {"item", Data.Items.Wood.ToDictionary()},
                {"xPosition", 20},
                {"yPosition", mid - 1 - y},
                {"currentHealth", 30}
            };
            savedWorldObjects.Add(newProp);
        } 
        worldDictionary.Add("SavedWorldObjects", savedWorldObjects);
        
        FileManager.SaveWorld(worldDictionary);
    }
}
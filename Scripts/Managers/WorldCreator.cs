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

        SavedObject stone = Data.SavedObjects.Stone;

        Random random = new();

        Array savedWorldObjects = new();
        for (int x = 0; x < worldBasicInfo.Width; x++) {
            for (int y = mid; y < worldBasicInfo.Height; y++) {
                Dictionary newBlock = new() {
                    { "savedObject", stone.ToDictionary() },
                    { "xPosition", x },
                    { "yPosition", y },
                };
                savedWorldObjects.Add(newBlock);
            }
        }

        worldDictionary.Add("SavedWorldObjects", savedWorldObjects);

        FileManager.SaveWorld(worldDictionary);
    }
}
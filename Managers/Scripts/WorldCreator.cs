using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Resources.Scripts;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Managers.Scripts;

public static class WorldCreator {
    // we are going to create a world dictionary
    // because the current method is degenerate

    public static void CreateWorld(WorldBasicInfo worldBasicInfo) {
        int mid = 7;
        BlockType stoneType = FileManager.LoadBlockType("res://Resources/BlockType/Stone.tres");
        BlockType earthType = FileManager.LoadBlockType("res://Resources/BlockType/Earth.tres");
        BlockType[] types = { stoneType, earthType };
        Random random = new();

        Array savedBlockArray = new();
        for (int x = 0; x < worldBasicInfo.Width; x++) {
            for (int y = mid; y < worldBasicInfo.Height; y++) {
                BlockType type = types[random.Next(2)];
                SavedBlock savedBlock = SavedBlock.Builder.New(type, x, y).Build();
                savedBlockArray.Add(savedBlock.Serialize());
            }
        }

        int defaultSpawnX = 5;
        int defaultSpawnY = 5;


        Dictionary worldDictionary = new();
        worldDictionary.Add("Name", worldBasicInfo.Name);
        worldDictionary.Add("Width", worldBasicInfo.Width);
        worldDictionary.Add("Height", worldBasicInfo.Height);
        worldDictionary.Add("SavedBlocks", savedBlockArray);
        worldDictionary.Add("PlayerPositions", new Array());
        worldDictionary.Add("DefaultSpawnPosition", new Array { defaultSpawnX, defaultSpawnY });
        FileManager.SaveWorld(worldDictionary);
    }
}
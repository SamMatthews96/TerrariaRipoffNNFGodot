using System;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public static class WorldCreator {
    public static World CreateWorld(WorldBasicInfo worldBasicInfo) {
        int mid = 7;
        BlockType stoneType = FileManager.LoadBlockType("res://Resources/BlockType/Stone.tres");
        
        BlockType earthType = FileManager.LoadBlockType("res://Resources/BlockType/Earth.tres");
        SavedBlock[,] savedBlocks = new SavedBlock[worldBasicInfo.Width, worldBasicInfo.Height];
        BlockType[] types = { stoneType, earthType };
        Random random = new();
        for (int x = 0; x < worldBasicInfo.Width; x++) {
            for (int y = mid; y < worldBasicInfo.Height; y++) {
                BlockType type = types[random.Next(2)];
                savedBlocks[x, y] = new SavedBlock(type, x, y);
            }
        }

        return new World(
            savedBlocks,
            worldBasicInfo.Name,
            worldBasicInfo.Width,
            worldBasicInfo.Height);
    }
}
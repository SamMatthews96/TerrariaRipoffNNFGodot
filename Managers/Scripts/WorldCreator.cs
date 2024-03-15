using System;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public static class WorldCreator {
    public static World CreateWorld(WorldBasicInfo worldBasicInfo) {
        int mid = 7;
        BlockType stoneType = ResourceLoader.Load<BlockType>("res://Resources/BlockType/Stone.tres");
        
        BlockType earthType = ResourceLoader.Load<BlockType>("res://Resources/BlockType/Earth.tres");
        SavedBlock[,] savedBlocks = new SavedBlock[worldBasicInfo.WorldWidth, worldBasicInfo.WorldHeight];
        BlockType[] types = { stoneType, earthType };
        Random random = new();
        for (int x = 0; x < worldBasicInfo.WorldWidth; x++) {
            for (int y = mid; y < worldBasicInfo.WorldHeight; y++) {
                BlockType type = types[random.Next(2)];
                savedBlocks[x, y] = new SavedBlock(type, x, y);
            }
        }

        return new World(
            savedBlocks,
            worldBasicInfo.Name,
            worldBasicInfo.WorldWidth,
            worldBasicInfo.WorldHeight);
    }
}
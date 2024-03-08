using System;
using System.Threading.Tasks;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public static class WorldCreator {
    public static World CreateWorld(WorldBasicInfo worldBasicInfo) {
        int mid = 7;
        BlockType blockType = ResourceLoader.Load<BlockType>("res://Resources/BlockType/Stone.tres");
        SavedBlock[,] savedBlocks = new SavedBlock[worldBasicInfo.WorldWidth, worldBasicInfo.WorldHeight];

        for (int x = 0; x < worldBasicInfo.WorldWidth; x++) {
            for (int y = mid; y < worldBasicInfo.WorldHeight; y++) {
                savedBlocks[x, y] = new SavedBlock(blockType, x, y);
            }
        }

        return new World(
            savedBlocks,
            worldBasicInfo.Name,
            worldBasicInfo.WorldWidth,
            worldBasicInfo.WorldHeight);
    }
}
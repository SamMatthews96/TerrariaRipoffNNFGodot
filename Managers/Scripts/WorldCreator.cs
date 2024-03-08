using System.Threading.Tasks;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public static class WorldCreator {
    public static async Task<World> CreateWorld(WorldBasicInfo worldBasicInfo) {
        int mid = 500;
        BlockType blockType = ResourceLoader.Load<BlockType>("res://Resources/BlockType/Stone.tres");
        SavedBlock[,] savedBlocks = new SavedBlock[worldBasicInfo.WorldWidth, worldBasicInfo.WorldHeight];

        int threads = 256;
        Task[] tasks = new Task[threads];
        for (int i = 0; i < threads; i++) {
            int xStart = i * worldBasicInfo.WorldWidth / threads;
            int xEnd = (i + 1) * worldBasicInfo.WorldWidth / threads;
            tasks[i] = Task.Run(() => {
                for (int x = xStart; x < xEnd; x++) {
                    for (int y = mid; y < worldBasicInfo.WorldHeight; y++) {
                        savedBlocks[x, y] = new SavedBlock(blockType, x, y);
                    }
                }
            });
        }

        await Task.WhenAll(tasks);

        return new World(
            savedBlocks, 
            worldBasicInfo.Name, 
            worldBasicInfo.WorldWidth, 
            worldBasicInfo.WorldHeight);
    }
}
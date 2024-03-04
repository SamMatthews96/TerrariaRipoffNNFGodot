using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Scenes;

public partial class WorldCreator : Node {
    [Signal]
    public delegate void WorldCreatedEventHandler(World world);

    private void OnCreateWorldButtonDown() {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        Task<World> createWorldTask = Task.Run(CreateWorld);
        createWorldTask.GetAwaiter().OnCompleted(() => {
            watch.Stop();
            GD.Print(watch.ElapsedMilliseconds);
            EmitSignal(SignalName.WorldCreated, createWorldTask.Result);
        });
    }

    private async Task<World> CreateWorld() {
        int worldWidth = 5000;
        int worldHeight = 1000;
        int mid = 500;
        BlockType blockType = ResourceLoader.Load<BlockType>("res://Resources/BlockType/Stone.tres");
        SavedBlock[,] savedBlocks = new SavedBlock[worldWidth, worldHeight];

        int threads = 256;
        Task[] tasks = new Task[threads];
        for (int i = 0; i < threads; i++) {
            int xStart = i * worldWidth / threads;
            int xEnd = (i + 1) * worldWidth / threads;
            tasks[i] = Task.Run(() => {
                for (int x = xStart; x < xEnd; x++) {
                    for (int y = mid; y < worldHeight; y++) {
                        savedBlocks[x, y] = new SavedBlock(blockType, x, y);
                    }
                }
            });
        }

        await Task.WhenAll(tasks);

        World newWorld = new World(savedBlocks, "Imma world", worldWidth, worldHeight);
        return newWorld;
    }
}
using System.Threading.Tasks;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Scenes; 

public partial class WorldCreator : Node {
	[Signal]
	public delegate void WorldCreatedEventHandler(World world);

	private void OnCreateWorldButtonDown() {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		int worldWidth = 1000;
		int worldHeight = 1000;
		int mid = 25;
        
		BlockType blockType = ResourceLoader.Load<BlockType>("res://Resources/BlockType/Stone.tres");
		SavedBlock [,] savedBlocks = new SavedBlock[worldWidth, worldHeight];
		
		for (int x = 0; x < worldWidth; x++) {
			for (int y = 0; y < mid; y++) {
				savedBlocks[x, y] = new SavedBlock(blockType, x, y);
			}
		}

		World newWorld = new World(savedBlocks, "Imma world", worldWidth, worldHeight);
		EmitSignal(SignalName.WorldCreated, newWorld);
		watch.Stop();

		GD.Print($"Execution Time: {watch.ElapsedMilliseconds} ms");
	}
}
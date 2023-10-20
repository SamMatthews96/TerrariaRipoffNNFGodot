using Godot;
using TerrariaRipoffNNF.scripts.BlockScripts;
using TerrariaRipoffNNF.scripts.Resources;
using static Godot.GD;

namespace TerrariaRipoffNNF.scripts.testScripts; 

public partial class TestBlockSpawner: Node2D {
	[Export] private BlockResource resource;
	
	public override void _Ready() {
		for (int i = 0; i < 20; i++) {
			Block.CreateBlock(i, 0, resource);
		}
		QueueFree();
	}
}

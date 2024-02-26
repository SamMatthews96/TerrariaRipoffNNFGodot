using Godot;
using System;

public partial class WorldData : Node {
	private SavedBlock[,] savedBlocks;
	[Export] private int worldWidth;
	[Export] private int worldHeight;

	[Export] private BlockType testBlockType;
	
	public override void _Ready() {
		savedBlocks = new SavedBlock[worldWidth, worldHeight];
		for (int x = 0; x < worldWidth; x++) {
			savedBlocks[x, 0] = new SavedBlock(testBlockType);
		}
	}
	
	
	
	
}

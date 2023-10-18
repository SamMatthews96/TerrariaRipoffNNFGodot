using Godot;
using static Godot.GD;
using TerrariaRipoffNNF.scripts.BlockScripts;

namespace TerrariaRipoffNNF.scripts.testScripts; 

public partial class TestBlockSpawner: Node2D {
    private BlockResource resource = Load<BlockResource>("res://BlockResources/stone.tres");
    
    public override void _Ready() {
        
        for (int i = 0; i < 20; i++) {
            Block.CreateBlock(i, 0, resource);
        }
        QueueFree();
    }
}
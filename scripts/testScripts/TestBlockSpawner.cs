using Godot;
using static Godot.GD;

namespace TerrariaRipoffNNF.scripts; 

public partial class TestBlockSpawner: Node2D {
    private BlockResource blockResource = new BlockResource {
        Weight = 10f,
        TensileStrength = 30f,
        Name = "test",
        MaxHealth = 50f,
    };
    public override void _Ready() {
        for (int i = 0; i < 20; i++) {
            Block.CreateBlock(i, 0, blockResource);
        }
    }
}
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF.Scripts.Managers;

public partial class BlockManager : Node {
    public static BlockManager Instance { get; private set; }

    private ActiveBlock[,] _activeBlocks;
    
    public override void _EnterTree() {
        if (Instance is not null) {
            throw new System.Exception("[20240814.1956.1] BlockManager already instantiated");
        }

        Instance = this;
    }

    public override void _Ready() {
        
    }
}
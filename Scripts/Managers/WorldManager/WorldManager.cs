using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class WorldManager : Node {
    public static WorldManager Create() {
        return Data.PackedScenes.WorldManager.Instantiate<WorldManager>();
    }
    
    [Export] public BlockManager BlockManager { get; private set; }
    [Export] public PickupManager PickupManager { get; private set; }
    private Game _game;

    public void SetGame(Game game) {
        if (_game is not null) {
            throw new Exception("[20250103.1823.1] Game already set");
        }
        _game = game;
        PickupManager.SetGame(game);
        BlockManager.SetGame(game);
    }
}
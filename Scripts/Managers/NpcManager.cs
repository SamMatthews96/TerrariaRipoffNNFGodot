using Godot;

namespace TerrariaRipoffNNF;

public partial class NpcManager : Node {
    [Export] private Game _game;
    
    public override void _Ready() {
        // listen to dev tool event
        _game.Interface.DevTools.SpawnPressed += OnSpawnPressed;
    }

    public override void _ExitTree() {
        _game.Interface.DevTools.SpawnPressed -= OnSpawnPressed;
    }

    private void OnSpawnPressed() {
        throw new System.NotImplementedException();
    }
}
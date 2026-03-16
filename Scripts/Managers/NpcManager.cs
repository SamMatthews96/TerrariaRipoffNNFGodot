using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;

namespace TerrariaRipoffNNF;

public partial class NpcManager : Node {
    [Export] private Game _game;
    
    public override void _Ready() {
        _game.Interface.DevTools.SpawnPressed += OnSpawnPressed;
    }

    public override void _ExitTree() {
        _game.Interface.DevTools.SpawnPressed -= OnSpawnPressed;
    }

    private void OnSpawnPressed() {
        EnemyNpc enemyNpc = EnemyNpc.Create(new IntVector(5,5));
        _game.World.AddChild(enemyNpc, true);
    }
}
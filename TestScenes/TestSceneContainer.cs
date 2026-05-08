using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;

namespace TerrariaRipoffNNF.TestScenes;

public partial class TestSceneContainer: Node2D {
    [Export] private Node2D _spawnPoint;
    [Export] private PackedScene _npcScene;
    public override void _Ready() {
        EnemyNpc newNpc =
            Data.PackedScenes.TestNpc.Instantiate<EnemyNpc>();
        _spawnPoint.AddChild(newNpc);
        EnemyNpc secondNpc = _npcScene.Instantiate<EnemyNpc>();
        _spawnPoint.AddChild(secondNpc);
    }
}
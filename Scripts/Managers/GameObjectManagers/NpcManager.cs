using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;

namespace TerrariaRipoffNNF;

public partial class NpcManager : Node2D {
    [Export] private World _world;
    [Export] private PackedScene _npcScene;
    private Interface.DevTools _devTools;

    public override void _Ready() {
        _devTools = _world.Interface.DevTools;

        _devTools.SpawnPressed += OnSpawnPressed;
        TreeExiting += () => {
            _devTools.SpawnPressed -= OnSpawnPressed;
        };
    }

    private void OnSpawnPressed() {
        EnemyNpc npc = EnemyNpc.Create(new Vector2I(20, 10));
        AddChild(npc);
    }
}
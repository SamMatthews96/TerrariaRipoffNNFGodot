using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;

namespace TerrariaRipoffNNF.Scripts.Managers.GameObjectManagers;

public partial class NpcManager : Node2D {
    [Export] private World _world;
    [Export] private PackedScene _npcScene;
    public override void _Ready() {
        _world.Interface.DevTools.SpawnPressed += OnSpawnPressed;
        TreeExiting += () => {
            _world.Interface.DevTools.SpawnPressed -= OnSpawnPressed;
        };
    }

    private void OnSpawnPressed() {
        EnemyNpc npc = EnemyNpc.Create(new Vector2I(20, 10));
        AddChild(npc);
    }
}
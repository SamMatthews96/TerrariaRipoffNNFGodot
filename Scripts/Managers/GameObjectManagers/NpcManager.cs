using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;

namespace TerrariaRipoffNNF;

public partial class NpcManager : Node2D {
    [Export] private World _world;
    [Export] private PackedScene _npcScene;
    private Interface.DevTools _devTools;
    private PlayerManager _playerManager;

    public override void _Ready() {
        _devTools = _world.Interface.DevTools;
        _playerManager = _world.PlayerManager;

        _playerManager.LocalPlayerSpawned += OnLocalPlayerSpawned;
        _devTools.SpawnPressed += OnSpawnPressed;
        TreeExiting += () => {
            _playerManager.LocalPlayerSpawned -= OnLocalPlayerSpawned;
            _devTools.SpawnPressed -= OnSpawnPressed;
        };
    }

    private void OnLocalPlayerSpawned(Player player) {
        player.WeaponSwung += OnWeaponSwung;
        player.TreeExiting += () => {
            player.WeaponSwung -= OnWeaponSwung;
        };
    }

    private void OnWeaponSwung(Vector2 position, Player player) {
        ItemWeapon weapon = player.PlayerEquipment.Weapon; 
        WeaponAttackNode node = 
            WeaponAttackNode.Create(weapon, player, position);
        AddChild(node);
    }

    private void OnSpawnPressed() {
        EnemyNpc npc = EnemyNpc.Create(new Vector2I(20, 10));
        AddChild(npc);
    }
}
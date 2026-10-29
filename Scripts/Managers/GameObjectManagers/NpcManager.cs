using System;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;
using TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

namespace TerrariaRipoffNNF;

public partial class NpcManager : Node2D {
    [Export] private World _world;
    [Export] private PackedScene _npcScene;
    private Interface.DevTools _devTools;
    private PlayerManager _playerManager;

    public event Action<Vector2I> NpcCreated;
    public event CellMovedDelegate NpcMoved;
    public event Action<Vector2I> NpcDestroyed;

    public override void _Ready() {
        _devTools = _world.Interface.DevTools;
        _playerManager = _world.PlayerManager;

        _playerManager.HostPlayerSpawned += OnPlayerSpawned;
        _devTools.SpawnPressed += OnSpawnPressed;
        TreeExiting += () => {
            _playerManager.HostPlayerSpawned -= OnPlayerSpawned;
            _devTools.SpawnPressed -= OnSpawnPressed;
        };
    }

    private void OnPlayerSpawned(Player player) {
        player.WeaponSwung += OnWeaponSwung;
        player.TreeExiting += () => { player.WeaponSwung -= OnWeaponSwung; };
    }

    private void OnWeaponSwung(Vector2 position, Player player) {
        int peerId = _playerManager.GetPlayerId(player);
        Rpc(nameof(RpcCreateWeaponSprite),
            position, peerId);
    }

    [Rpc(CallLocal = true)]
    private void RpcCreateWeaponSprite(Vector2 position, int playerId) {
        Player player = _playerManager.GetPlayer(playerId);
        ItemWeapon weapon = player.PlayerEquipment.Weapon; 
        WeaponAttackNode node = 
            WeaponAttackNode.Create(weapon, player, position);
        AddChild(node);
        if (node is WeaponAttackProjectile projectile) {
            NpcCreated?.Invoke(projectile.Coords);
            projectile.MovedCell += OnNpcMovedCell;
        
            projectile.TreeExiting += () => {
                NpcDestroyed?.Invoke(projectile.Coords);
                projectile.MovedCell -= OnNpcMovedCell;
            };
        }
    }

    private void OnNpcMovedCell(Vector2I newCoords, Vector2I oldCoords) {
        NpcMoved?.Invoke(newCoords, oldCoords);
    }

    private void OnSpawnPressed() {
        EnemyNpc npc = EnemyNpc.Create(new Vector2I(20, 10));
        AddChild(npc);
    }
}
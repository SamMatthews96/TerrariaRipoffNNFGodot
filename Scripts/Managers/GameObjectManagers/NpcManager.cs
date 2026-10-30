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
    private int _count = 0;

    public event Action<Vector2I> NpcCreated;
    public event CellMovedDelegate NpcMoved;
    public event Action<Vector2I> NpcDestroyed;

    private int GetNodeId(Node node) {
        return node.Name.ToString().ToInt();
    }

    private Node GetNode(int id) {
        return GetNode(id.ToString());
    }

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
        this.RpcHost(nameof(RpcHostAttackAction),
            position, _playerManager.GetPlayerId(player)
        );
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcHostAttackAction(Vector2 position, int playerId) {
        this.RpcAll(nameof(SpawnWeaponNode), position, playerId, ++_count);
    }

    [Rpc(CallLocal = true)]
    private void SpawnWeaponNode(Vector2 position, int playerId, int id) {
        Player player = _playerManager.GetPlayer(playerId);
        ItemWeapon weapon = player.PlayerEquipment.Weapon;
        WeaponAttackNode node =
            WeaponAttackNode.Create(weapon, player, position);
        node.Name = id.ToString();
        AddChild(node);
        if (!player.World.IsHost) return;
        if (node is WeaponAttackProjectile projectile) {
            void BodyEnteredAction(Node body) {
                HostDespawnProjectile(GetNodeId(projectile));
            }

            void TimeoutAction() {
                HostDespawnProjectile(GetNodeId(projectile));
            }

            NpcCreated?.Invoke(projectile.Coords);

            projectile.MovedCell += OnNpcMovedCell;
            projectile.Timeout += TimeoutAction;
            projectile.BodyEntered += BodyEnteredAction;
            projectile.TreeExiting += () => {
                NpcDestroyed?.Invoke(projectile.Coords);

                projectile.MovedCell -= OnNpcMovedCell;
                projectile.Timeout -= TimeoutAction;
                projectile.BodyEntered -= BodyEnteredAction;
            };
        }
    }

    private void HostDespawnProjectile(int id) {
        this.RpcAll(nameof(RpcDespawnProjectile), id);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcDespawnProjectile(int id) {
        Node node = GetNode(id);
        node.QueueFree();
    }

    private void OnNpcMovedCell(Vector2I newCoords, Vector2I oldCoords) {
        NpcMoved?.Invoke(newCoords, oldCoords);
    }

    private void OnSpawnPressed() {
        this.RpcHost(nameof(RpcHostSpawnEnemy), ++_count);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcHostSpawnEnemy(int id) {
        this.RpcAll(nameof(RpcPeerSpawnEnemy), id);
    }

    [Rpc(CallLocal = true)]
    private void RpcPeerSpawnEnemy(int id) {
        EnemyNpc npc = EnemyNpc.Create(new Vector2I(20, 10), id);
        AddChild(npc);
        if (_world.IsHost) {
            npc.HitZeroHealth += OnHitZeroHealth;
        }
    }

    private void OnHitZeroHealth(EnemyNpc npc) {
        npc.HitZeroHealth -= OnHitZeroHealth;
        this.RpcAll(nameof(RpcDeleteNode), GetNodeId(npc));
    }

    [Rpc(CallLocal = true)]
    private void RpcDeleteNode(int id) {
        Node npc = GetNode(id);
        npc.QueueFree();
    }
}
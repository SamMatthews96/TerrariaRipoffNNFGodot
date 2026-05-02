using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.TestScenes;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class World : Node2D {
    public Game Game { get; private set; }
    public Vector2I WorldSize { get; private set; }
    public bool IsHost { get; private set; }
    public Vector2I DefaultSpawnPosition { get; private set; } = new(4, 14);
    [Export] public PickupManager PickupManager { get; private set; }
    [Export] public PlayerManager PlayerManager { get; private set; }
    [Export] public PropManager PropManager { get; private set; }
    [Export] public InputManager InputManager { get; private set; }
    [Export] public Interface.Game Interface { get; private set; }
    [Export] public BlockManager BlockManager { get; private set; }
    [Export] public ItemIdBimap ItemIdBimap { get; private set; }
    
    private Dictionary _localPlayerData;
    public Dictionary WorldData { get; private set; }

    public static World CreateAsHost(Game game, Dictionary worldData, Dictionary playerData) {
        World world = Data.PackedScenes.World.Instantiate<World>();
        world.IsHost = true;
        world.WorldSize = new Vector2I((int)worldData["Width"], (int)worldData["Height"]);
        world.Game = game;
        world._localPlayerData = playerData;
        world.WorldData = worldData;

        return world;
    }

    public static World CreateAsClient(Dictionary metadata, Dictionary playerData, Game game) {
        World world = Data.PackedScenes.World.Instantiate<World>();
        world.WorldSize = new Vector2I((int)metadata["Width"], (int)metadata["Height"]);
        world.Game = game;
        world.WorldData = metadata;
        world._localPlayerData = playerData;
        return world;
    }

    private void OnExitGameClicked() {
        Visible = false;
        Interface.GameMenu.ExitGameButtonDown -= OnExitGameClicked;
        QueueFree();
    }

    public override void _Ready() {
        Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
        TreeExiting += () => { Interface.GameMenu.ExitGameButtonDown -= OnExitGameClicked; };
        if (IsHost) {
            PlayerManager.SpawnHostPlayer(_localPlayerData);
            _localPlayerData = null;
        } else {
            BlockManager.SyncComplete += ClientOnSyncComplete;
            BlockManager.ClientGetWorldData();
        }
    }

    public bool IsInBounds(Vector2I coords) {
        return coords.X >= 0
               && coords.X < WorldSize.X
               && coords.Y >= 0
               && coords.Y < WorldSize.Y;
    }

    public bool IsCellFilled(Vector2I coords) {
        if (BlockManager.Blocks[coords.X, coords.Y] is not null) return true;
        return PropManager.PropCells.ContainsKey(coords);
    }

    public bool IsInOrthogonalRange(Vector2I a, Vector2I b, int range) {
        if (!IsInBounds(a) || !IsInBounds(b)) return false;
        if (Math.Abs(a.X - b.X) > range) return false;
        if (Math.Abs(a.Y - b.Y) > range) return false;
        return true;
    }

    public CellEntity GetPriorityCellEntity(
        Vector2I coords, Array<CellEntity> types) {
        foreach (CellEntity type in types) {
            switch (type) {
                case CellEntity.Block:
                    if (BlockManager.Blocks[coords.X, coords.Y] is not null) {
                        return type;
                    }
                    break;
                case CellEntity.Prop:
                    if (PropManager.PropCells.ContainsKey(coords)) {
                        return type;
                    }
                    break;
                case CellEntity.Wall:
                    if (BlockManager.Walls[coords.X, coords.Y] is not null) {
                        return type;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return CellEntity.None;
    }

    private void ClientOnSyncComplete() {
        PlayerManager.SpawnPlayersOnClient(_localPlayerData);
        _localPlayerData = null;
        BlockManager.SyncComplete -= ClientOnSyncComplete;
    }
}
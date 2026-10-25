using System;
using Godot;
using Godot.Collections;
using System.Threading.Tasks;

namespace TerrariaRipoffNNF;

public partial class GameUnloader : Node {
    private readonly World _world;
    private BlockManager _blockManager;
    private PropManager _propManager;
    private ItemIdBimap _itemIdBimap;
    private Vector2I _worldSize;
    private bool _completedSave;

    public event Action SaveComplete;

    public GameUnloader(World world) {
        _world = world;
    }

    public GameUnloader() { }

    public override void _Ready() {
        _blockManager = _world.BlockManager;
        _propManager = _world.PropManager;
        _itemIdBimap = _world.ItemIdBimap;
        _worldSize = _world.WorldSize;

        Task.Run(SaveWorld);
    }

    public override void _Process(double delta) {
        if (!_completedSave) return;
        SaveComplete?.Invoke();
    }

    private void SaveWorld() {
        Dictionary worldData = _world.SerializeWorld();
        FileManager.SaveWorld(worldData);
        _completedSave = true;
    }

}
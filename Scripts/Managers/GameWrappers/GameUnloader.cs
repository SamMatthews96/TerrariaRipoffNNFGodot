using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;
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
        // Serialize blocks, walls, and props in parallel
        Task<Dictionary<string, Dictionary>> blocksTask =
            Task.Run(() => _blockManager.SerializeBlocks());
        Task<Dictionary<string, Dictionary>> wallsTask =
            Task.Run(() => _blockManager.SerializeWalls());
        Task<Dictionary<string, Dictionary>> propsTask =
            Task.Run(SerializeProps);

        Task.WaitAll(blocksTask, wallsTask, propsTask);

        Dictionary worldData = new() {
            ["Name"] = _world.WorldData["Name"],
            ["Width"] = _worldSize.X,
            ["Height"] = _worldSize.Y,
            ["blocks"] = blocksTask.Result,
            ["walls"] = wallsTask.Result,
            ["props"] = propsTask.Result,
            ["itemMap"] = _itemIdBimap.ToDictionary()
        };

        FileManager.SaveWorld(worldData);
        _completedSave = true;
    }



    private Dictionary<string, Dictionary> SerializeProps() {
        Dictionary<string, Dictionary> groupedByItemId = new();

        foreach ((Vector2I coords, ActiveProp prop) in _propManager.Props) {
            Item item = prop.Item;
            string itemId = _itemIdBimap.GetId(item).ToString();

            if (!groupedByItemId.ContainsKey(itemId)) {
                groupedByItemId[itemId] = new Dictionary();
            }

            if (!groupedByItemId[itemId].ContainsKey($"{coords.X}")) {
                groupedByItemId[itemId][$"{coords.X}"] = new Array();
            }

            ((Array)groupedByItemId[itemId][$"{coords.X}"]).Add(coords.Y);
        }

        return groupedByItemId;
    }
}
using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Environment = System.Environment;

namespace TerrariaRipoffNNF;

public partial class GameUnloader : Node {
    private readonly World _world;
    private bool _completedSave;

    public event Action SaveComplete;

    public GameUnloader(World world) {
        _world = world;
    }

    public GameUnloader() { }

    public override void _Ready() {
        Task.Run(SaveWorld);
    }

    public override void _Process(double delta) {
        if (!_completedSave) return;
        SaveComplete?.Invoke();
    }

    private void SaveWorld() {
        // Serialize blocks, walls, and props in parallel
        Task<Dictionary<string, Dictionary>> blocksTask =
            Task.Run(() =>
                SerializeBlocks(_world.BlockManager.Blocks));
        Task<Dictionary<string, Dictionary>> wallsTask =
            Task.Run(() =>
                SerializeBlocks(_world.BlockManager.Walls));
        Task<Dictionary<string, Dictionary>> propsTask =
            Task.Run(SerializeProps);

        Task.WaitAll(blocksTask, wallsTask, propsTask);

        Dictionary worldData = new() {
            ["Name"] = _world.WorldData["Name"],
            ["Width"] = _world.WorldSize.X,
            ["Height"] = _world.WorldSize.Y,
            ["blocks"] = blocksTask.Result,
            ["walls"] = wallsTask.Result,
            ["props"] = propsTask.Result,
            ["itemMap"] = _world.ItemIdBimap.ToDictionary()
        };

        FileManager.SaveWorld(worldData);
        _completedSave = true;
    }

    private Dictionary<string, Dictionary> SerializeBlocks(Block[,] data) {
        Dictionary<string, Dictionary> groupedByItemId = new();
        int itemCount = _world.ItemIdBimap.getItemCount();
        for (int i = 0; i < itemCount; i++) {
            string idStr = i.ToString();
            groupedByItemId[idStr] = new Dictionary();
        }
        
        for (int x = 0; x < _world.WorldSize.X; x++) {
            string xStr = x.ToString();

            // Pre-create arrays for this column and cache references
            Array[] columnArrays = new Array[itemCount];
            for (int i = 0; i < itemCount; i++) {
                string idStr = i.ToString();
                Array arr = new();
                groupedByItemId[idStr][xStr] = arr;
                columnArrays[i] = arr;
            }

            for (int y = 0; y < _world.WorldSize.Y; y++) {
                Block block = data[x, y];
                if (block is null) continue;

                columnArrays[block.ItemId].Add(y);
            }
        }

        return groupedByItemId;
    }

    private Dictionary<string, Dictionary> SerializeProps() {
        Dictionary<string, Dictionary> groupedByItemId = new();

        foreach ((Vector2I coords, Prop prop) in _world.PropManager.Props) {
            Item item = prop.Item;
            string itemId = _world.ItemIdBimap.GetId(item).ToString();

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
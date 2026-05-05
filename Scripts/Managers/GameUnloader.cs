using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;
using System.Threading.Tasks;

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
        Dictionary worldData = new() {
            ["Name"] = _world.WorldData["Name"],
            ["Width"] = _world.WorldSize.X,
            ["Height"] = _world.WorldSize.Y,
            ["blocks"] = SerializeBlocks(_world.BlockManager.Blocks),
            ["walls"] = SerializeBlocks(_world.BlockManager.Walls),
            ["props"] = SerializeProps(),
            ["itemMap"] = _world.ItemIdBimap.ToDictionary()
        };

        FileManager.SaveWorld(worldData);
        _completedSave = true;
    }

    private Dictionary<string, Dictionary> SerializeBlocks(Block[,] data) {
        Dictionary<string, Dictionary> groupedByItemId = new();

        for (int x = 0; x < _world.WorldSize.X; x++) {
            for (int y = 0; y < _world.WorldSize.Y; y++) {
                Block block = data[x, y];
                if (block is null) continue;
                string idStr = block.ItemId.ToString();
                if (!groupedByItemId.ContainsKey(idStr)) {
                    groupedByItemId[idStr] = new Dictionary();
                }

                if (!groupedByItemId[idStr].ContainsKey($"{x}")) {
                    groupedByItemId[idStr][$"{x}"] = new Array();
                }

                ((Array)groupedByItemId[idStr][$"{x}"]).Add(y);
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
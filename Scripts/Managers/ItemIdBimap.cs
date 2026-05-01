using System;
using System.Text;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.TestScenes;

public partial class ItemIdBimap : Node {
    private Array<string> _idToString = new();
    private Dictionary<string, ushort> _stringToId = new();
    private Dictionary<string, Item> _stringToItem = new();

    [Export] private World _world;

    public override void _Ready() {
        Dictionary dict = _world.WorldData["itemMap"].AsGodotDictionary();
        _idToString = dict["IdToString"].AsGodotArray<string>();
        _stringToId = dict["StringToId"].AsGodotDictionary<string, ushort>();

        Dictionary<string, Dictionary> stringToItemDict =
            dict["StringToItem"].AsGodotDictionary<string, Dictionary>();
        foreach ((string key, Dictionary itemDict) in stringToItemDict) {
            _stringToItem[key] = Item.FromDictionary(itemDict);
        }
    }

    public ushort GetId(Item item) {
        string itemString = GetItemString(item);
        bool isIdSet = _stringToId.TryGetValue(itemString, out ushort id);
        if (isIdSet) return id;

        if (!_world.IsHost) throw new Exception("Item not found on client");
        ushort count = (ushort)_idToString.Count;
        Rpc(nameof(RpcAddItem),
            count, itemString, item.ToDictionary());
        return count;
    }

    [Rpc(CallLocal = true)]
    private void RpcAddItem(ushort id, string itemString, Dictionary itemDict) {
        Item item = Item.FromDictionary(itemDict);
        if (_idToString.Count != id) {
            throw new Exception("Id mismatch");
        }

        _idToString.Add(itemString);
        _stringToId[itemString] = id;
        _stringToItem[itemString] = item;
    }


    public Item GetItem(ushort id) {
        string itemString = _idToString[id];
        return _stringToItem[itemString];
    }

    private string GetItemString(Item item) {
        if (!item.TryGetProperty(out ItemCrafted itemCrafted)) {
            return item.ResourcePath;
        }

        string recipeString = $"{itemCrafted.Recipe.Id}";
        foreach (
            (string slot, Item ingredient)
            in itemCrafted.SuppliedIngredients
        ) {
            recipeString += $"|{slot}-{GetId(ingredient)}";
        }

        return recipeString;
    }

    public Dictionary ToDictionary() {
        Dictionary<string, Dictionary> stringToItemDict = new();
        foreach ((string key, Item item) in _stringToItem) {
            stringToItemDict[key] = item.ToDictionary();
        }

        return new Dictionary {
            { "IdToString", _idToString },
            { "StringToId", _stringToId },
            { "StringToItem", stringToItemDict },
        };
    }
}
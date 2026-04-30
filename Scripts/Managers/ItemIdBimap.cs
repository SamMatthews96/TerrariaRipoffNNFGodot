using System;
using System.Text;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.TestScenes;

// potential concern: ordering of dict elements

public class ItemIdBimap {
    private readonly Array<string> _idToString = new();
    private readonly Dictionary<string, ushort> _stringToId = new();
    private readonly Dictionary<string, Item> _stringToItem = new();
    
    public ItemIdBimap() {    }

    public ItemIdBimap(Dictionary dict) {
        _idToString = dict["IdToString"].AsGodotArray<string>();
        _stringToId = dict["StringToId"].AsGodotDictionary<string, ushort>();
        
        // _stringToItem =
        Dictionary<string, Dictionary> stringToItemDict =
            dict["StringToItem"].AsGodotDictionary<string, Dictionary>();
        foreach ((string key, Dictionary itemDict) in stringToItemDict) {
            _stringToItem[key] = Item.FromDictionary(itemDict);
        }
    }
    
    public ushort GetId(Item item) {
        string itemString = GetItemString(item);
        if (_stringToId.TryGetValue(itemString, out ushort id)) {
            return id;
        }

        ushort count = (ushort)_idToString.Count;
        _idToString.Add(itemString);
        _stringToId[itemString] = count;
        _stringToItem[itemString] = item;
        return count;
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
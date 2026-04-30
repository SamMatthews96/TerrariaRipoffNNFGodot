using System;
using System.Text;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.TestScenes;

// potential concern: ordering of dict elements

public class ItemIdBimap {
    private Array<string> _idToString = new();
    private Dictionary<string, UInt16> _stringToId = new();
    private Dictionary<string, Item> _stringToItem = new();
    
    public ItemIdBimap() {    }

    public ItemIdBimap(Dictionary dict) {
        _idToString = dict["IdToString"].AsGodotArray<string>();
        _stringToId = dict["StringToId"].AsGodotDictionary<string, UInt16>();
        
        // _stringToItem =
        Dictionary<string, Dictionary> stringToItemDict =
            dict["StringToItem"].AsGodotDictionary<string, Dictionary>();
        foreach ((string key, Dictionary itemDict) in stringToItemDict) {
            _stringToItem[key] = Item.FromDictionary(itemDict);
        }
    }
    
    public UInt16 GetId(Item item) {
        string itemString = GetItemString(item);
        if (_stringToId.TryGetValue(itemString, out UInt16 id)) {
            return id;
        }

        _idToString.Add(itemString);
        UInt16 count = (UInt16)(_idToString.Count - 1);
        _stringToId[itemString] = count;
        _stringToItem[itemString] = item;
        return count;
    }

    public Item GetItem(UInt16 id) {
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
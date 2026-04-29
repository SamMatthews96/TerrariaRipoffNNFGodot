using System;
using Godot.Collections;

namespace TerrariaRipoffNNF.TestScenes;

public class ItemIdBimap {
    private Dictionary<string, int> _stringToId = new();
    private Dictionary<int, string> _idToString = new();

    public ItemIdBimap() { }

    public int GetId(string name) {
        if (_stringToId.TryGetValue(name, out int id)) return id;
        int count = _stringToId.Count;
        _stringToId.Add(name, count);
        _idToString.Add(count, name);
        return _stringToId[name];
    }

    public string GetEncodedItem(int id) {
        return !_idToString.TryGetValue(id, out string item)
            ? throw new Exception("item id not found: " + id)
            : item;
    }
}
using System;
using System.Text;
using System.Threading.Tasks;
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

        _world.PlayerManager.PlayerSpawnedOnHost += OnPlayerSpawnedOnHost;
        TreeExiting += () => { _world.PlayerManager.PlayerSpawnedOnHost -= OnPlayerSpawnedOnHost; };
    }

    public Item GetItem(ushort id) {
        string itemString = _idToString[id];
        return _stringToItem[itemString];
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

    public bool AreItemsSame(Item item1, Item item2) {
        return GetItemString(item1) == GetItemString(item2);
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

    private void OnPlayerSpawnedOnHost(Player player) {
        foreach (StackedItems stackedItems in player.Inventory.StackedItemsList) {
            Item item = stackedItems.Item;
            string itemString = GetItemString(item);
            if (!_stringToId.ContainsKey(itemString)) {
                ushort id = (ushort)_idToString.Count;
                Rpc(nameof(RpcAddItem),
                    id, itemString, item.ToDictionary());
            }
        }
    }

    [Rpc(CallLocal = true,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcAddItem(ushort id, string itemString, Dictionary itemDict) {
        Item item = Item.FromDictionary(itemDict);
        if (item is null) {
            throw new Exception("Item is null");
        }
        if (_idToString.Count != id) {
            throw new Exception("Id mismatch");
        }

        _idToString.Add(itemString);
        _stringToId[itemString] = id;
        _stringToItem[itemString] = item;

        if (_stringToId.Count != _idToString.Count) {
            throw new Exception("count mismatch");
        }
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
}
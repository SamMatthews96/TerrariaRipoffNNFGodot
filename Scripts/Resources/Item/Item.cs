using Godot;
using Godot.Collections;
using Exception = System.Exception;

namespace TerrariaRipoffNNF;

[GlobalClass]
public sealed partial class Item : Resource {
    [Export] public string Name { get; private set; }
    [Export] public float InventorySpace { get; private set; }
    [Export] public bool IsStackable { get; private set; } = true;
    [Export] public Texture2D IconTexture { get; private set; }
    [Export] private Array<ItemProperty> _itemProperties = new();

    public static bool AreEqual(Item a, Item b) {
        if (a.ResourcePath != "" || b.ResourcePath != "") {
            return a.ResourcePath == b.ResourcePath;
        }
        if (
            a.TryGetProperty(out ItemCrafted aItemCrafted) &&
            b.TryGetProperty(out ItemCrafted bItemCrafted)
        ) {
            return ItemCrafted.AreEqual(aItemCrafted, bItemCrafted);
        }

        return false;
    }

    public T GetProperty<T>() where T : ItemProperty {
        if (TryGetProperty(out T property)) {
            return property;
        }

        throw new Exception($"Item does not have property of type {typeof(T)}");
    }

    public bool TryGetProperty<T>(out T property) where T : ItemProperty {
        foreach (ItemProperty itemProperty in _itemProperties) {
            if (itemProperty is not T castedProperty) continue;
            property = castedProperty;
            return true;
        }

        property = null;
        return false;
    }

    public bool HasProperty<T>() where T : ItemProperty {
        return TryGetProperty(out T _);
    }

    public Dictionary ToDictionary() {
        if (ResourcePath != "") {
            return new Dictionary {
                { "ResourcePath", ResourcePath },
            };
        }

        return GetProperty<ItemCrafted>().ToDictionary();
    }

    public Dictionary<string, Dictionary> GetTooltipAttributes() {
        Dictionary<string, Dictionary> newDictionary = new();
        foreach (ItemProperty itemProperty in _itemProperties) {
            Dictionary tooltipAttributes = itemProperty.GetTooltipAttributes();
            newDictionary.Add(tooltipAttributes["PropertyName"].ToString(), tooltipAttributes);
        }

        return newDictionary;
    }

    public static Item FromDictionary(Dictionary dictionary) {
        if (dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
            return ResourceLoader.Load<Item>(resourcePath.AsString());
        } 
        else {
            // string recipeResourcePath = dictionary["RecipeResourcePath"].AsString();
            // Recipe recipe = ResourceLoader.Load<Recipe>(recipeResourcePath);
            // Dictionary<string, Item> suppliedIngredients = new();
            // dictionary["SuppliedIngredients"].AsGodotDictionary<string, Dictionary>();
            // foreach ((string key, Dictionary itemDict) in dictionary["SuppliedIngredients"]
            //              .AsGodotDictionary<string, Dictionary>()) {
            //     Item item = FromDictionary(itemDict);
            //     suppliedIngredients.Add(key, item);
            // }
            // Item newItem = recipe.Build(suppliedIngredients).Item;
            // return newItem;
            throw new Exception("Not implemented");
        }
    }

    public static Item Create(
        string name, Texture2D iconTexture, float inventorySpace, bool isStackable = true,
        Array<ItemProperty> itemProperties = null) {
        return new Item {
            Name = name,
            IconTexture = iconTexture,
            InventorySpace = inventorySpace,
            IsStackable = isStackable,
            _itemProperties = itemProperties ?? new Array<ItemProperty>()
        };
    }
}
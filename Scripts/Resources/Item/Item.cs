using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

[GlobalClass]
public sealed partial class Item : Resource {
    [Export] public string Name { get; private set; }
    [Export] public float InventorySpace { get; private set; } = 0;
    [Export] public bool IsStackable { get; private set; } = true;
    [Export] public Texture2D IconTexture { get; private set; }
    [Export] private Array<ItemProperty> _itemProperties = new();

    public T GetProperty<T>() where T : ItemProperty {
        if (TryGetProperty(out T property)) {
            return property;
        }

        throw new KeyNotFoundException($"Item does not have property of type {typeof(T)}");
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
        return TryGetProperty(out T property);
    }

    public Dictionary ToDictionary() {
        Dictionary serialized = new();
        if (ResourcePath == "") {
            serialized.Add("Name", Name);
            serialized.Add("InventorySpace", InventorySpace);
            serialized.Add("IsStackable", IsStackable);
            serialized.Add("IconTexture", IconTexture.ResourcePath);
            Array serializedProperties = new();
            foreach (ItemProperty property in _itemProperties) {
                serializedProperties.Add(property.ToDictionary());
            }

            serialized.Add("ItemProperties", serializedProperties);
        } else {
            serialized.Add("ResourcePath", ResourcePath);
        }

        return serialized;
    }

    public Godot.Collections.Dictionary<string, Dictionary> GetTooltipAttributes() {
        Godot.Collections.Dictionary<string, Dictionary> newDictionary = new();
        foreach (ItemProperty itemProperty in _itemProperties) {
            newDictionary.Add(itemProperty.PropertyType.ToString(), itemProperty.GetTooltipAttributes());
        }

        return newDictionary;
    }

    public static Item FromDictionary(Dictionary dictionary) {
        if (dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
            return ResourceLoader.Load<Item>(resourcePath.ToString());
        }

        Array<ItemProperty> itemProperties = new();

        foreach (Dictionary serializedProperty in dictionary["ItemProperties"].AsGodotArray()) {
            itemProperties.Add(ItemProperty.FromDictionary(serializedProperty));
        }

        return Create(
            dictionary["Name"].ToString(),
            ResourceLoader.Load<Texture2D>(dictionary["IconTexture"].ToString()),
            (float)dictionary["InventorySpace"],
            (bool)dictionary["IsStackable"],
            itemProperties
        );
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
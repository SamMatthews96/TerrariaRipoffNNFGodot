using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

[GlobalClass]
public sealed partial class Item : Resource {
    [Export] public float InventorySpace { get; private set; }
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

    public static Item FromDictionary(Dictionary dictionary) {
        Item newItem;
        if (dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
            newItem = ResourceLoader.Load<Item>(resourcePath.ToString());
        } else {
            throw new NotImplementedException();
        }
        
        foreach (ItemProperty property in newItem._itemProperties) {
            // property.
        }
        
        return newItem;
    }
}
using System;
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

        throw new NullReferenceException($"Item does not have property of type {typeof(T)}");
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
        if (dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
            return ResourceLoader.Load<Item>(resourcePath.ToString());
        }
        throw new NotImplementedException();
    }

    public static Builder New() {
        return new Builder();
    }

    public class Builder {
        private readonly Item _item = new();

        public Item Build() {
            return _item;
        }

        public Builder SetName(string name) {
            _item.Name = name;
            return this;
        }

        public Builder AddProperty(ItemProperty property) {
            _item._itemProperties.Add(property);
            return this;
        }

        public Builder SetIconTexture(Texture2D iconTexture) {
            _item.IconTexture = iconTexture;
            return this;
        }

        public Builder SetInventorySpace(float inventorySpace) {
            _item.InventorySpace = inventorySpace;
            return this;
        }

        public Builder SetIsStackable(bool isStackable) {
            _item.IsStackable = isStackable;
            return this;
        }

        // public 
    }
}
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemPlaceable : ItemProperty {
    [Export] public Array<IntVector> OccupiedCells { get; private set; }
    [Export] public Texture2D Texture { get; private set; }

    [Export] private Array<PlaceableProperty> _placeableProperties = new();
    
    public T GetProperty<T>() where T : PlaceableProperty {
        if (TryGetProperty(out T property)) {
            return property;
        }

        throw new KeyNotFoundException($"Item does not have property of type {typeof(T)}");
    }
    
    public bool TryGetProperty<T>(out T property) where T : PlaceableProperty {
        foreach (PlaceableProperty placeableProperty in _placeableProperties) {
            if (placeableProperty is not T castedProperty) continue;
            property = castedProperty;
            return true;
        }

        property = null;
        return false;
    }
    
    public override Dictionary ToDictionary() {
        Dictionary serialized = new();
        serialized.Add("ResourcePath", ResourcePath);
        return serialized;
    }

    public override Dictionary GetTooltipAttributes() {
        return new Dictionary();
    }

    public static ItemPlaceable Create(Texture2D texture, Array<IntVector> cells) {
        ItemPlaceable itemPlaceable = new() {
            Texture = texture,
            OccupiedCells = cells
        };
        return itemPlaceable;
    }
}
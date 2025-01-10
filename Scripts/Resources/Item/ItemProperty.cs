using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public enum PropertyType {
    Mining,
    Block,
    Ingredient,
    Placeable
}

[GlobalClass]
public abstract partial class ItemProperty : Resource {
    public abstract PropertyType PropertyType { get; }
    public abstract Dictionary ToDictionary();
    public abstract Dictionary GetTooltipAttributes();

    public static ItemProperty FromDictionary(Dictionary dictionary) {
        if (dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
            return ResourceLoader.Load<ItemProperty>(resourcePath.ToString());
        }

        return dictionary["PropertyType"].ToString() switch {
            "Mining" => ItemMining.FromDictionary(dictionary),
            _ => throw new NotImplementedException()
        };
    }
}

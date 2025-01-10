using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class ItemProperty : Resource {
    public abstract Dictionary ToDictionary();
    public abstract Dictionary GetTooltipAttributes();

    public static ItemProperty FromDictionary(Dictionary dictionary) {
        if (dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
            return ResourceLoader.Load<ItemProperty>(resourcePath.ToString());
        }

        return dictionary["PropertyType"].ToString() switch {
            nameof(ItemMining) => ItemMining.FromDictionary(dictionary),
            _ => throw new NotImplementedException()
        };
    }
}

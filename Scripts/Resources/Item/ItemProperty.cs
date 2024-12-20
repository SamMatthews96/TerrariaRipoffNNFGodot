using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public enum PropertyType {
    Mining,
    Block,
    Ingredient
}

[GlobalClass]
public abstract partial class ItemProperty : Resource {
    public abstract PropertyType PropertyType { get; }
    public abstract Dictionary ToDictionary();
    public abstract Dictionary GetTooltipAttributes();

    public static ItemProperty FromDictionary(Dictionary dictionary) {
        if (dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
            return ResourceLoader.Load<ItemProperty>(resourcePath.ToString());
        } else {
            throw new NotImplementedException(
                "ItemProperty.FromDictionary not implemented for non-ResourcePath properties");
        }
    }
}
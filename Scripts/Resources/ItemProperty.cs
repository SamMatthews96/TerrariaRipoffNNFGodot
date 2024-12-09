using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public abstract partial class ItemProperty : Resource {
    // public Item ParentItem { get; private set; }
    public abstract Dictionary ToDictionary();

    public static ItemProperty FromDictionary(Item item, Dictionary dictionary) {
        ItemProperty newProperty;
        if (dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
            newProperty = ResourceLoader.Load<ItemProperty>(resourcePath.ToString());
        } else {
            throw new NotImplementedException(
                "ItemProperty.FromDictionary not implemented for non-ResourcePath properties");
        }
        
        // newProperty.ParentItem = item;
        return newProperty;
    }
}
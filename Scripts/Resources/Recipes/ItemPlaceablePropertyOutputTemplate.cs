using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemPlaceablePropertyOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public Texture2D Texture { get; private set; }
    [Export] public Array<IntVector> OccupiedCells { get; private set; } 
    [Export] public PlaceableType Type { get; private set; }
    

    public override ItemProperty Build(
        Dictionary<string, Item> suppliedIngredients,
        Dictionary<string, RecipeIngredientSlot> ingredientSlots
    ) {
        return new ItemPlaceable(Texture, OccupiedCells,Type);
    }
}
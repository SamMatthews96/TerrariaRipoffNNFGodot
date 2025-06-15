using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemIngredient : ItemProperty {
    [Export] public IngredientType IngredientType { get; private set; }
    [Export] public float Quality { get; private set; }
    [Export] public string Name { get; private set; }
    
    public override Dictionary GetTooltipAttributes() {
        return new Dictionary();
    }
}
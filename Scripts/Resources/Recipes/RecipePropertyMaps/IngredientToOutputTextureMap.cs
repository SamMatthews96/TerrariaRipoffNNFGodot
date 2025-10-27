using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class IngredientToOutputTextureMap : Resource{
    [Export] public Dictionary<ItemIngredient, Texture2D> Map;
}
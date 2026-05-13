using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemOutputTemplate : Resource {
    [Export] public RecipeFieldMapString Name { get; private set; }
    [Export] public RecipeFieldMapTexture Texture { get; private set; }
    [Export] public RecipeFieldMapFloat Space { get; private set; }
    [Export] public Array<ItemPropertyOutputTemplate> Properties { get; private set; }
}
using Godot;

namespace TerrariaRipoffNNF.TestScenes.MapDebugging;

[GlobalClass]
public partial class Recipe : Resource {
    [Export] private int _number;
    [Export] private RecipeFieldMapFloat _field;
    [Export] private RecipeFieldMapFloatConstant _constant;
}
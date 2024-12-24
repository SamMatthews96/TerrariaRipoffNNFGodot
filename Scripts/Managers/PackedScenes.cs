using Godot;
using TerrariaRipoffNNF.Interface;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PackedScenes : Resource {
    
    [Export] public PackedScene Player { get; private set; }
    [Export] public PackedScene ActiveBlock { get; private set; }
    [Export] public PackedScene ActivePickup { get; private set; }
    [Export] public PackedScene MainMenu { get; private set; }
    [Export] public PackedScene LoadScreen { get; private set; }
    [Export] public PackedScene Game { get; private set; }
    [Export] public PackedScene SelectCraftingStationButton { get; private set; }
    [Export] public PackedScene SelectRecipeButton { get; private set; }
    [Export] public PackedScene RecipeIngredientSlotTexture { get; private set; }
    [Export] public PackedScene BlockTypeButton { get; private set; }
    [Export] public PackedScene ItemTooltipPropertyGroup { get; private set; }
    [Export] public PackedScene SelectIngredientButton { get; private set; }
    
}
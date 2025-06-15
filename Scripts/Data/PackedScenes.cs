using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PackedScenes : Resource {
    [Export] public PackedScene Player { get; private set; }
    [Export] public PackedScene WorldPickup { get; private set; }
    [Export] public PackedScene WorldSolid { get; private set; }
    [Export] public PackedScene MainMenu { get; private set; }
    [Export] public PackedScene LoadScreen { get; private set; }
    [Export] public PackedScene Game { get; private set; }
    [Export] public PackedScene SelectCraftingStationButton { get; private set; }
    [Export] public PackedScene SelectRecipeButton { get; private set; }
    [Export] public PackedScene RecipeIngredientSlotTexture { get; private set; }
    [Export] public PackedScene BlockTypeButton { get; private set; }
    [Export] public PackedScene ItemTooltipPropertyGroup { get; private set; }
    [Export] public PackedScene SelectIngredientButton { get; private set; }
    // Managers
    [Export] public PackedScene LoadingScreen { get; private set; }
    [Export] public PackedScene WorldObjectManager { get; private set; }
}
using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PackedScenes : Resource {
    [Export] public PackedScene Player { get; private set; }
    [Export] public PackedScene TestNpc { get; private set; }
    [Export] public PackedScene PlayerActionController { get; private set; }
    [Export] public PackedScene PlayerPickupArea { get; private set; }
    [Export] public PackedScene PlayerCrafting { get; private set; }
    [Export] public PackedScene WorldObject { get; private set; }
    [Export] public PackedScene WorldPickup { get; private set; }
    [Export] public PackedScene WorldSolid { get; private set; }
    [Export] public PackedScene WorldStatic { get; private set; }
    [Export] public PackedScene CraftStationArea { get; private set; }
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
    
    // WeaponSprites
    [Export] public PackedScene WeaponProjectile { get; private set; }
    [Export] public PackedScene WeaponSwing { get; private set; }
    [Export] public PackedScene PickaxeSwing { get; private set; }
}
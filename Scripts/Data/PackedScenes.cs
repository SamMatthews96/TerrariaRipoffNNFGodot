using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PackedScenes : Resource {
    [Export] public PackedScene Player { get; private set; }
    [Export] public PackedScene TestNpc { get; private set; }
    [Export] public PackedScene PlayerPickupArea { get; private set; }
    [Export] public PackedScene Pickup { get; private set; }
    [Export] public PackedScene Prop { get; private set; }
    // UI
    [Export] public PackedScene SelectCraftingStationButton { get; private set; }
    [Export] public PackedScene SelectRecipeButton { get; private set; }
    [Export] public PackedScene RecipeIngredientSlotTexture { get; private set; }
    [Export] public PackedScene BlockTypeButton { get; private set; }
    [Export] public PackedScene ItemTooltipPropertyGroup { get; private set; }
    [Export] public PackedScene SelectIngredientButton { get; private set; }
    
    // Managers
    [Export] public PackedScene World { get; private set; }
    
    // WeaponSprites
    [Export] public PackedScene WeaponProjectile { get; private set; }
    [Export] public PackedScene WeaponSwing { get; private set; }
    [Export] public PackedScene PickaxeSwing { get; private set; }
}
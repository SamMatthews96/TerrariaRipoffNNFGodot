using System.Linq;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class Recipe : Resource {
    [Export] public CraftingStationType CraftingStationType { get; private set; }
    [Export] public Dictionary<string, RecipeIngredientSlot> IngredientSlots { get; private set; }

    // Output qualities
    [Export] public string Name { get; private set; }
    [Export] public bool IsStackable { get; private set; }
    [Export] public RecipePropertyMapString ResultNameMap { get; private set; }
    [Export] public RecipePropertyMapMultiplier InventorySpaceMap { get; private set; }
    [Export] public Array<ItemPropertyOutputTemplate> ItemProperties { get; private set; }
    [Export] public RecipePropertyMapTexture ResultTextureMap { get; private set; }
    [Export] public Texture2D TemplateIcon { get; private set; }

    public StackedItems Build(Dictionary<string, Item> suppliedIngredients) {
        if (IngredientSlots.Keys.Any(key => !suppliedIngredients.ContainsKey(key))) {
            return null;
        }

        Array<ItemProperty> newItemProperties = new() {
            new ItemCrafted(this, suppliedIngredients)
        };
        foreach (ItemPropertyOutputTemplate itemPropertyOutputTemplate in ItemProperties) {
            ItemProperty newItemProperty
                = itemPropertyOutputTemplate.Build(suppliedIngredients, IngredientSlots);
            newItemProperties.Add(newItemProperty);
        }
        
        string name = ResultNameMap.ResolveTemplate(suppliedIngredients);
        Texture2D iconTexture = ResultTextureMap.ResolveTemplate(suppliedIngredients);
        float inventorySpace = InventorySpaceMap.ResolveTemplate(suppliedIngredients);

        Item item = Item.Create(
            name: name,
            iconTexture: iconTexture,
            inventorySpace: inventorySpace,
            isStackable: IsStackable,
            itemProperties: newItemProperties
        );
        return new StackedItems(item);
    }
}
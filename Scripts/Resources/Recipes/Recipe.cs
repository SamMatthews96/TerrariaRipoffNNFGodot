using System.Linq;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class Recipe : Resource {
    [Export] public CraftingStationType CraftingStationType { get; private set; }
    [Export] public string Name { get; private set; }
    [Export] public Dictionary<string, RecipeIngredientSlot> IngredientSlots { get; private set; }
    [Export] public RecipePropertyMapString ResultNameMap { get; private set; }
    [Export] public RecipePropertyMapMultiplier InventorySpace { get; private set; }
    [Export] public bool IsStackable { get; private set; }
    [Export] public Array<ItemPropertyOutputTemplate> ItemProperties { get; private set; }
    
    [Export] public Texture2D ResultIcon { get; private set; }

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
        

        Item item = Item.Create(
            name: ResultNameMap.ResolveTemplate(suppliedIngredients, IngredientSlots),
            iconTexture: ResultIcon,
            inventorySpace: InventorySpace.ResolveTemplate(suppliedIngredients, IngredientSlots),
            isStackable: IsStackable,
            itemProperties: newItemProperties
        );
        return new StackedItems(item);
    }
}
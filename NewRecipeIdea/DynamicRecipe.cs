using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class DynamicRecipe : Resource {
    [Export] public Array<RecipeIngredientSlot> IngredientSlots { get; private set; }
    [Export] public RecipeOutputTemplate OutputTemplate { get; private set; }

    public Item BuildFromTemplate(
        Dictionary<string, Item> suppliedIngredients
    ) {
        // start by making a static item, then try mapping a simple property
        // let's say it weights more if the wood is better

        RecipePropertyMapMultiplier propertyMap = OutputTemplate.InventorySpace;
        float inventorySpace = propertyMap.ResolveTemplate(suppliedIngredients, IngredientSlots);
        
        
        Item newItem = Item.Create(
            name: "TempItem",
            iconTexture: new Texture2D(),
            inventorySpace: inventorySpace, 
            isStackable: false
        );


        return newItem;
    }
}
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
        float inventorySpace = OutputTemplate.InventorySpace
            .ResolveTemplate(suppliedIngredients, IngredientSlots);
        string name = OutputTemplate.Name
            .ResolveTemplate(suppliedIngredients, IngredientSlots);

        Item newItem = Item.Create(
            name: name,
            iconTexture: new Texture2D(),
            inventorySpace: inventorySpace,
            isStackable: false
        );


        return newItem;
    }
}
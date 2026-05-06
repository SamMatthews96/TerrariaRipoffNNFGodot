using System.Linq;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class Recipe : Resource {
    [Export] public int Id;
    [Export] public string RecipeName { get; private set; }
    [Export] public StationType RequiredStation { get; private set; }
    [Export] public Dictionary<string, Ingredient> RecipeIngredients { get; private set; }
    [Export] public Texture2D TemplateIcon { get; private set; }

    [Export] public bool IsStackable { get; private set; }
    [Export] public RecipeFieldMapString ResultNameMap { get; private set; }
    [Export] public RecipeFieldMapFloat ResultInventorySpaceMap { get; private set; }
    [Export] public RecipeFieldMapTexture ResultTextureSingleMap { get; private set; }
    [Export] public Array<ItemPropertyOutputTemplate> ResultItemProperties { get; private set; }

    public StackedItems Build(Dictionary<string, Item> suppliedIngredients) {
        if (RecipeIngredients.Keys.Any(key => !suppliedIngredients.ContainsKey(key))) {
            return null;
        }
    
        Array<ItemProperty> newItemProperties = new() {
            new ItemCrafted(this, suppliedIngredients)
        };
        foreach (ItemPropertyOutputTemplate template in ResultItemProperties) {
            ItemProperty newItemProperty
                = template.Build(suppliedIngredients);
            newItemProperties.Add(newItemProperty);
        }
    
        string name = ResultNameMap.ResolveTemplate(suppliedIngredients);
        Texture2D iconTexture = ResultTextureSingleMap.ResolveTemplate(suppliedIngredients);
        float inventorySpace = ResultInventorySpaceMap.ResolveTemplate(suppliedIngredients);
    
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
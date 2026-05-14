using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class Recipe : Resource {
    [Export] public int Id { get; private set; }
    [Export] public string RecipeName { get; private set; }
    [Export] public StationType RequiredStation { get; private set; }
    [Export] public Texture2D TemplateIcon { get; private set; }
    [Export] public Dictionary<string, Ingredient> RecipeIngredients { get; private set; }
    [Export] public ItemOutputTemplate ItemOutputTemplate { get; private set; }
    
    public StackedItems Build(Dictionary<string, Item> suppliedIngredients) {
        Item item = new(
            this, ItemOutputTemplate, suppliedIngredients);
        
        return new StackedItems(item);
    }
}
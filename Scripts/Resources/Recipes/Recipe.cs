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
        
    /* Example
     *  Ingredients: Dictionary<string, XXX>
     *      wood: Wood
     *
     *  OutputTemplate
     *      Name: StringMap
     *      InventorySpace: FloatMap
     *      Texture: TextureMap
     *      Prop.Texture: TextureMap
     *      Prop.Dimensions: Vector2IMap
     *      Prop.Station.Type: EnumMap
     */
    public StackedItems Build(Dictionary<string, Item> suppliedIngredients) {
        Item item = new(ItemOutputTemplate, suppliedIngredients);
        return new StackedItems(item);
    }
}
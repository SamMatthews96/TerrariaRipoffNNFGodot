using System.Linq;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;



[GlobalClass]
public partial class ItemIngredient : ItemProperty {
    public override Dictionary ToDictionary() {
        throw new System.NotImplementedException();
    }
    
    [Export] private Array<IngredientProperty> _ingredientProperties = new();
    
    public IngredientProperty GetProperty(IngredientType ingredientType) {
        return _ingredientProperties.First(property => property.IngredientType == ingredientType);
    }
}
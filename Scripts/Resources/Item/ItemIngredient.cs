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
    
    public bool HasProperty(IngredientType type) {
        return _ingredientProperties.Any(property => property.IngredientType == type);
    }
    
    public IngredientProperty GetProperty(IngredientType type) {
        return _ingredientProperties.First(property => property.IngredientType == type);
    }
    
    public bool TryGetProperty(IngredientType type, out IngredientProperty property) {
        property = _ingredientProperties.First(property => property.IngredientType == type);
        return property != null;
    }
    
    
    
}
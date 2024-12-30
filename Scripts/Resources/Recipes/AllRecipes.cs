using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class AllRecipes : Resource {
    [Export] private Array<CraftingStationRecipes> _recipesArray;
    private Dictionary<CraftingStationType, CraftingStationRecipes> _recipes;

    private Dictionary<CraftingStationType, CraftingStationRecipes> Recipes {
        get {
            if (_recipes is null) {
                _recipes = new Dictionary<CraftingStationType, CraftingStationRecipes>();
                foreach (var craftingStationRecipes in _recipesArray) {
                    _recipes[craftingStationRecipes.CraftingStationType] = craftingStationRecipes;
                }
            }

            return _recipes;
        }
        set => _recipes = value;
    }
    
    public Array<Recipe> GetRecipes(CraftingStationType craftingStationType) {
        return Recipes[craftingStationType].Recipes;
    }
}
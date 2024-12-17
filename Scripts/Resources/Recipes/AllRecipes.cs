using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class AllRecipes : Resource {
    [Export] private Array<CraftingStationRecipes> _recipesArray;
    private Dictionary<CraftingStationType, CraftingStationRecipes> _recipes;

    public Dictionary<CraftingStationType, CraftingStationRecipes> Recipes {
        get {
            if (_recipes is null) {
                _recipes = new Dictionary<CraftingStationType, CraftingStationRecipes>();
                foreach (var craftingStationRecipes in _recipesArray) {
                    _recipes[craftingStationRecipes.CraftingStationType] = craftingStationRecipes;
                }
            }

            return _recipes;
        }
        private set => _recipes = value;
    }
}
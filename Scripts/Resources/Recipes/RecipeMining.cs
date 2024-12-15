using System;
using System.ComponentModel;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Recipes;

/* How will it work in the game
  Player opens crafting menu
  For now, all crafts will use no crafting station: hands/none;
  Player sees a list of Recipes


 */

[GlobalClass]
public partial class RecipeMining : Recipe {
    [Export] private Texture2D _ferriumPickTexture;
    [Export] private float _speedCoefficient = 0.5f;
    [Export] private float _powerCoefficient = 10f;

    public override Item Build(Dictionary<string, Item> suppliedIngredients) {
        SuppliedIngredients = suppliedIngredients;

        IngredientProperty metal = GetIngredientType("pickaxeHead");
        IngredientProperty wood = GetIngredientType("pickaxeHandle");

        string newItemName;
        Texture2D newItemTexture;
        switch (metal.Name) {
            case "Ferrium":
                newItemName = "Ferrium Pickaxe";
                newItemTexture = _ferriumPickTexture;
                break;
            default:
                throw new InvalidEnumArgumentException("[20241215.1545.1] Unknown metal type: " + metal.Name);
        }

        return Item.New()
            .SetName(newItemName)
            .SetIconTexture(newItemTexture)
            .SetInventorySpace(5f)
            .SetIsStackable(false)
            .AddProperty(
                ItemMining.New()
                    .SetMiningSpeed(wood.Quality * _speedCoefficient)
                    .SetRange(8f)
                    .SetMiningPower(metal.Quality * _powerCoefficient)
                    .Build()
            ).Build();
    }
}
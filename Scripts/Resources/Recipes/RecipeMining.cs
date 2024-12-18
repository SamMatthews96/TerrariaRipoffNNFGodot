using System.ComponentModel;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

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

        string newItemName = $"{metal.Name} Pickaxe";
        Texture2D newItemTexture;
        switch (metal.Name) {
            case "Ferrium":
                newItemTexture = _ferriumPickTexture;
                break;
            default:
                throw new InvalidEnumArgumentException("[20241215.1545.1] Unknown metal type: " + metal.Name);
        }

        SuppliedIngredients = null;

        return Item.Create(
            name: newItemName,
            iconTexture: newItemTexture,
            inventorySpace: 5f,
            isStackable: false,
            itemProperties: new Array<ItemProperty> {
                ItemMining.Create(
                    speed: wood.Quality * _speedCoefficient,
                    range: 8f,
                    power: metal.Quality * _powerCoefficient
                )
            });
    }
}
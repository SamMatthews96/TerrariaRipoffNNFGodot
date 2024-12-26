using System.ComponentModel;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipeMining : Recipe {
    [Export] private Texture2D _ferriumPickTexture;
    [Export] private float _speedCoefficient = 0.5f;
    [Export] private float _powerCoefficient = 10f;

    public override StackedItems Build(Dictionary<string, Item> suppliedIngredients) {

        IngredientProperty metal = GetIngredientType("pickaxeHead", suppliedIngredients);
        IngredientProperty wood = GetIngredientType("pickaxeHandle", suppliedIngredients);
        if (metal is null || wood is null) {
            return null;
        }

        string newItemName = $"{metal.Name} Pickaxe";
        Texture2D newItemTexture;
        switch (metal.Name) {
            case "Ferrium":
                newItemTexture = _ferriumPickTexture;
                break;
            default:
                throw new InvalidEnumArgumentException("[20241215.1545.1] Unknown metal type: " + metal.Name);
        }

        Item newPickaxe = Item.Create(
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
        return new StackedItems(newPickaxe);
    }
}
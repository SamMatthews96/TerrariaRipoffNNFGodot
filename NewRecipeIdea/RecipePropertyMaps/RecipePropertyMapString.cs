using System;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipePropertyMapString : RecipePropertyMap<string> {
    [Export] private string _stringTemplate;

    [Export] private Dictionary<string, IngredientNameToOutputNameMap> _ingredientNameMaps = new();

    public override string ResolveTemplate(
        Dictionary<string, Item> suppliedIngredients,
        Array<RecipeIngredientSlot> ingredientSlots
    ) {
        MatchCollection templateVariables = TemplateVariableRegex().Matches(_stringTemplate);
        string currentTemplate = _stringTemplate;
        foreach (object templateVariable in templateVariables) {
            string templateString = templateVariable.ToString();
            if (templateString == null) {
                throw new Exception("[20250518.1531.1] Invalid string template: " + _stringTemplate);
            }

            string recipeSlotName = templateString.Substring(1, templateString.Length - 2);

            RecipeIngredientSlot ingredientSlot = ingredientSlots.First(
                recipeIngredientSlot => recipeIngredientSlot.RecipeSlot == recipeSlotName);
            IngredientType ingredientType = ingredientSlot.IngredientType;

            Item item = suppliedIngredients[recipeSlotName];
            string ingredientName = item.GetProperty<ItemIngredient>()
                .GetProperty(ingredientType).Name;

            string templateSubstitute = ingredientName;
            if (_ingredientNameMaps.TryGetValue(recipeSlotName, out IngredientNameToOutputNameMap ingredientNameMap)) {
                templateSubstitute = ingredientNameMap.Map[ingredientName];
            }

            currentTemplate = currentTemplate.Replace("{" + recipeSlotName +"}", templateSubstitute);
        }

        return currentTemplate;
    }

    [GeneratedRegex(@"\{([^{}]*)\}")]
    private static partial Regex TemplateVariableRegex();
}
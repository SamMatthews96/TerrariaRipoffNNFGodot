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
        Dictionary<string, RecipeIngredientSlot> ingredientSlots
    ) {
        MatchCollection templateVariables = TemplateVariableRegex().Matches(_stringTemplate);
        string currentTemplate = _stringTemplate;
        foreach (object templateVariable in templateVariables) {
            string templateString = templateVariable.ToString();
            if (templateString == null) {
                throw new Exception("[20250518.1531.1] Invalid string template: " + _stringTemplate);
            }

            string recipeSlotName = templateString.Substring(1, templateString.Length - 2);

            Item item = suppliedIngredients[recipeSlotName];
            ItemIngredient itemIngredient = item.GetProperty<ItemIngredient>();
            string templateSubstitute = _ingredientNameMaps.TryGetValue(
                recipeSlotName,
                out IngredientNameToOutputNameMap ingredientNameMap
            )
                ? ingredientNameMap.Map[itemIngredient]
                : itemIngredient.Name;

            currentTemplate = currentTemplate.Replace("{" + recipeSlotName + "}", templateSubstitute);
        }

        return currentTemplate;
    }

    [GeneratedRegex(@"\{([^{}]*)\}")]
    private static partial Regex TemplateVariableRegex();
}
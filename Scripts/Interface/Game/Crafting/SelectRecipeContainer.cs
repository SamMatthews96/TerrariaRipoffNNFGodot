using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class SelectRecipeContainer : Control {
    [Export] public CraftingInterface CraftingInterface { get; private set; }
    [Export] private Container _recipeContainer;
    private Array _recipes = new();

    public override void _Ready() {
        CraftingInterface.CraftStationContainer.CraftingStationButtonClicked +=
            OnCraftingStationButtonClicked;
    }

    private void OnCraftingStationButtonClicked(CraftingStation craftingStation) {
        // Change the display of Handcrafted Recipes
        // remove all current recipes
        // add recipes from allRecipes[craftingStation.Type)
        foreach (Recipe recipe in Manager.Instance.AllRecipes
                     .Recipes[craftingStation.Type].Recipes) {
            // Create Button add handlers

            SelectRecipeButton newButton = SelectRecipeButton.Create(recipe);
            
            _recipeContainer.AddChild(newButton);
        }
        // .ForEach(recipe => {
        // RecipeButton newButton = RecipeButton.Create(recipe);
        // newButton.RecipeButtonClicked += OnRecipeButtonClicked;
        // _recipeContainer.AddChild(newButton);
        // });
    }
}
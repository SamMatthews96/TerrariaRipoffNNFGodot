using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class RecipeContainer : Control {
    [Export] public Crafting CraftingInterface { get; private set; }
    [Export] private Container _selectRecipeButtonContainer;
    private readonly List<RecipeButton> _recipeSelectButtons = new();

    // public event Action<Recipe> RecipeButtonClicked;

    public override void _Ready() {
        foreach (Node node in _selectRecipeButtonContainer.GetChildren()) {
            node.QueueFree();
        }

        Hide();
        CraftingInterface.StationContainer.CraftingStationButtonClicked +=
            OnCraftingCraftStationButtonClicked;
    }

    private void OnCraftingCraftStationButtonClicked(CraftingStation craftingStation) {
        _recipeSelectButtons.ForEach(button => {
            // button.RecipeButtonClicked -= OnRecipeButtonClicked;
            button.QueueFree();
        });
        _recipeSelectButtons.Clear();

        // foreach (Recipe recipe in Data.Recipes.GetRecipes(craftingStation.Type)) {
        //     RecipeButton newButton = RecipeButton.Create(recipe);
        //     _recipeSelectButtons.Add(newButton);
        //     newButton.RecipeButtonClicked += OnRecipeButtonClicked;
        //     _selectRecipeButtonContainer.AddChild(newButton);
        // }

        Show();
    }

    // private void OnRecipeButtonClicked(Recipe recipe) {
    //     RecipeButtonClicked?.Invoke(recipe);
    // }
}
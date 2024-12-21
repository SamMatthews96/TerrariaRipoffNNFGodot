using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectRecipeContainer : Control {
    [Export] public Crafting CraftingInterface { get; private set; }
    [Export] private Container _recipeContainer;
    private readonly List<SelectRecipeButton> _recipeSelectButtons = new();
    
    public event Action<Recipe> RecipeButtonClicked;

    public override void _Ready() {
        foreach (Node node in _recipeContainer.GetChildren()) {
            node.QueueFree();
        }
        Hide();
        CraftingInterface.CraftStationContainer.CraftingStationButtonClicked +=
            OnCraftingStationButtonClicked;
    }

    private void OnCraftingStationButtonClicked(CraftingStation craftingStation) {
        _recipeSelectButtons.ForEach(button => {
            button.RecipeButtonClicked -= OnRecipeButtonClicked;
            button.QueueFree();
        });
        _recipeSelectButtons.Clear();
        
        foreach (Recipe recipe in Manager.Instance.AllRecipes
                     .GetRecipes(craftingStation.Type)) {
            SelectRecipeButton newButton = SelectRecipeButton.Create(recipe);
            _recipeSelectButtons.Add(newButton);
            newButton.RecipeButtonClicked += OnRecipeButtonClicked;
            _recipeContainer.AddChild(newButton);
        }
        
        Show();
    }

    private void OnRecipeButtonClicked(Recipe recipe) {
        RecipeButtonClicked?.Invoke(recipe);
    }
}
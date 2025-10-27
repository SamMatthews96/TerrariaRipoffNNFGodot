using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectRecipeButton : TextureButton {
    private Recipe _recipe;
    public event Action<Recipe> RecipeButtonClicked;
    
    public static SelectRecipeButton Create(Recipe recipe) {
        SelectRecipeButton button = 
            Data.PackedScenes.SelectRecipeButton
                .Instantiate<SelectRecipeButton>();
        button._recipe = recipe;
        button.TextureNormal = recipe.TemplateIcon;
        return button;
    }
    
    public override void _Ready() {
        ButtonDown += OnButtonDown;
    }
    
    public override void _ExitTree() {
        ButtonDown -= OnButtonDown;
    }
    
    private void OnButtonDown() {
        RecipeButtonClicked?.Invoke(_recipe);
    }
}
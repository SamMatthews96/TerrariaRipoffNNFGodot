using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectIngredientPopup : Control {
    [Export] private Container _iconContainer;
    [Export] private SelectIngredientsContainer _selectIngredientsContainer;

    private RecipeIngredientSlot _ingredientSlot;
    private List<SelectIngredientButton> _ingredientButtons = new();
    private Player _player;

    public event Action<Item, RecipeIngredientSlot> IngredientSelected;

    public override void _Ready() {
        Hide();
        foreach (Node child in _iconContainer.GetChildren()) {
            child.QueueFree();
        }

        _selectIngredientsContainer.IngredientIconMouseEntered += OnIngredientIconMouseEntered;
        _selectIngredientsContainer.IngredientIconMouseLeft += OnIngredientIconMouseLeft;

        Player.BeforeLocalPlayerSpawned += OnLocalPlayerSpawned;
    }

    private void OnLocalPlayerSpawned(Player player) {
        _player = player;
    }


    private void OnIngredientIconMouseEntered(Control node, RecipeIngredientSlot ingredientSlot) {
        _player.Inventory.StackedItemsList.ForEach(stackedItems => {
            if (!stackedItems.Item.TryGetProperty(out ItemIngredient itemIngredient)) return;
            if (!itemIngredient.HasProperty(ingredientSlot.IngredientType)) return;
            SelectIngredientButton newButton = SelectIngredientButton.Create(stackedItems.Item);
            _ingredientButtons.Add(newButton);
            _iconContainer.AddChild(newButton);
            newButton.IngredientSelected += OnIngredientSelected;
        });
        
        _ingredientSlot = ingredientSlot;
        Position = node.GlobalPosition + new Vector2(0, -Size.Y);
        Show();
    }

    private void OnIngredientSelected(Item item) {
        IngredientSelected?.Invoke(item, _ingredientSlot);
        Hide();
    }

    private void OnIngredientIconMouseLeft() {
        foreach (SelectIngredientButton selectIngredientButton in _ingredientButtons) {
            selectIngredientButton.QueueFree();
        }

        _ingredientButtons.Clear();
        Hide();
    }
}
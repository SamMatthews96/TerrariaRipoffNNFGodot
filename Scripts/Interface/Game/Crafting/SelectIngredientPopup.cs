using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectIngredientPopup : Control {
    [Export] private Container _iconContainer;
    [Export] private SelectIngredientsContainer _selectIngredientsContainer;
    [Export] private Timer _hideTimer;
    [Export] private PanelContainer _panelContainer;

    private RecipeIngredientSlot _ingredientSlot;
    private List<SelectIngredientButton> _ingredientButtons = new();
    private Player _player;
    private bool _isMouseOverPopup;
    private bool _isMouseOverIngredientMouseover;

    public event Action<Item, RecipeIngredientSlot> IngredientButtonClicked;

    public override void _Ready() {
        Hide();
        foreach (Node child in _iconContainer.GetChildren()) {
            child.QueueFree();
        }

        _selectIngredientsContainer.IngredientIconMouseEntered += OnIngredientIconMouseEntered;
        _selectIngredientsContainer.IngredientIconMouseLeft += OnIngredientIconMouseLeft;

        Player.BeforeLocalPlayerSpawned += OnLocalPlayerSpawned;

        _hideTimer.Timeout += OnHideTimerTimeout;
        _panelContainer.MouseEntered += OnMouseEntered;
        _panelContainer.MouseExited += OnMouseExited;
    }

    private void OnHideTimerTimeout() {
        Hide();
    }

    private void OnLocalPlayerSpawned(Player player) {
        _player = player;
    }

    private void OnMouseExited() {
        _isMouseOverPopup = false;
        OnMouseoverUpdated();
    }

    private void OnMouseEntered() {
        _isMouseOverPopup = true;
        OnMouseoverUpdated();
    }

    private void OnIngredientIconMouseEntered(Control node, RecipeIngredientSlot ingredientSlot) {
        int addedMargin = 200;
        _isMouseOverIngredientMouseover = true;
        _ingredientSlot = ingredientSlot;
        Position = node.GlobalPosition + new Vector2(0, -Size.Y -addedMargin );
        OnMouseoverUpdated();
    }

    private void OnIngredientIconMouseLeft() {
        _isMouseOverIngredientMouseover = false;
        OnMouseoverUpdated();
    }

    private void OnMouseoverUpdated() {
        bool isMouseOver = _isMouseOverIngredientMouseover || _isMouseOverPopup;

        if (isMouseOver) {
            _hideTimer.Stop();
            if (!Visible) {
                OpenPopup();
            }
        }
        else {
            if (Visible) {
                _hideTimer.Start();
            }
        }
    }

    private void OpenPopup() {
        foreach (SelectIngredientButton selectIngredientButton in _ingredientButtons) {
            selectIngredientButton.IngredientButtonClicked -= OnIngredientButtonClicked;
            selectIngredientButton.QueueFree();
        }

        _ingredientButtons.Clear();

        _player.Inventory.StackedItemsList.ForEach(stackedItems => {
            if (!stackedItems.Item.TryGetProperty(out ItemIngredient itemIngredient)) return;
            if (!itemIngredient.HasProperty(_ingredientSlot.IngredientType)) return;
            SelectIngredientButton newButton = SelectIngredientButton.Create(stackedItems.Item);
            _ingredientButtons.Add(newButton);
            _iconContainer.AddChild(newButton);
            newButton.IngredientButtonClicked += OnIngredientButtonClicked;
        });

        Show();
        _hideTimer.Stop();
    }

    private void OnIngredientButtonClicked(Item item) {
        _isMouseOverIngredientMouseover = false;
        _isMouseOverPopup = false;
        _hideTimer.Stop();
        IngredientButtonClicked?.Invoke(item, _ingredientSlot);
        Hide();
    }
}
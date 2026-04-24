using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectIngredientPopup : Window {
    [Export] private Crafting _craftingInterface;
    [Export] private Container _buttonContainer;
    [Export] private Timer _hideTimer;
    [Export] private PanelContainer _panelContainer;

    private RecipeIngredientSlot _ingredientSlot;
    private readonly List<SelectIngredientButton> _ingredientButtons = new();
    private Player _player;
    private bool _isMouseOverPopup;
    private bool _isMouseOverIngredientMouseover;

    private SelectIngredientsContainer _selectIngredientsContainer;

    [Export] private SelectIngredientsContainer SelectIngredientsContainer {
        get => _selectIngredientsContainer;
        set {
            if (_selectIngredientsContainer is not null) {
                throw new Exception("SelectIngredientsContainer already set");
            }

            _selectIngredientsContainer = value;
            _selectIngredientsContainer.IngredientIconMouseEntered += OnIngredientIconMouseEntered;
            _selectIngredientsContainer.IngredientIconMouseLeft += OnIngredientIconMouseLeft;

            TreeExiting += () => {
                _selectIngredientsContainer.IngredientIconMouseEntered -= OnIngredientIconMouseEntered;
                _selectIngredientsContainer.IngredientIconMouseLeft -= OnIngredientIconMouseLeft;
            };
        }
    }

    public event Action<Item, RecipeIngredientSlot> SelectIngredientButtonClicked;

    public override void _Ready() {
        _craftingInterface.GameInterface.World.PlayerManager.LocalPlayerSpawned += 
            OnLocalPlayerSpawned;
        _hideTimer.Timeout += OnHideTimerTimeout;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;

        TreeExiting += () => {
            _craftingInterface.GameInterface.World.PlayerManager.LocalPlayerSpawned -=
                OnLocalPlayerSpawned;
            _hideTimer.Timeout -= OnHideTimerTimeout;
            MouseEntered -= OnMouseEntered;
            MouseExited -= OnMouseExited;
        };
    }

    private void OnHideTimerTimeout() {
        Hide();
    }

    private void OnLocalPlayerSpawned(Player player) {
        foreach (Node child in _buttonContainer.GetChildren()) {
            child.QueueFree();
        }

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
        _isMouseOverIngredientMouseover = true;
        _ingredientSlot = ingredientSlot;
        Position = new Vector2I {
            X = (int)node.GlobalPosition.X,
            Y = (int)(node.GlobalPosition.Y + node.Size.Y - 10)
        };
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
            OpenPopup();
        } else {
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

        _player?.Inventory.StackedItemsList.ForEach(stackedItems => {
            if (!stackedItems.Item.TryGetProperty(out ItemIngredient itemIngredient)) return;
            if (itemIngredient.IngredientType != _ingredientSlot.IngredientType) return;
            SelectIngredientButton newButton = SelectIngredientButton.Create(stackedItems.Item);
            _ingredientButtons.Add(newButton);
            _buttonContainer.AddChild(newButton);
            newButton.IngredientButtonClicked += OnIngredientButtonClicked;
        });

        Size = new Vector2I {
            X = (int)_panelContainer.Size.X,
            Y = (int)_panelContainer.Size.Y
        };
        Show();

        _hideTimer.Stop();
    }

    private void OnIngredientButtonClicked(Item item) {
        _isMouseOverIngredientMouseover = false;
        _isMouseOverPopup = false;
        _hideTimer.Stop();
        SelectIngredientButtonClicked?.Invoke(item, _ingredientSlot);
        Hide();
    }
}
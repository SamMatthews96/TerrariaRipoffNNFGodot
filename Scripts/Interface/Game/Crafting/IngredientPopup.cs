using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class IngredientPopup : Window {
    [Export] private Crafting _craftingInterface;
    [Export] private Container _buttonContainer;
    [Export] private Timer _hideTimer;
    [Export] private PanelContainer _panelContainer;

    private readonly List<IngredientButton> _ingredientButtons = new();
    private Player _player;
    private bool _isMouseOverPopup;
    private bool _isMouseOverIngredientMouseover;
    private string _slotName;

    public event Action<Item, string> SelectIngredientButtonClicked;

    public override void _Ready() {
        _craftingInterface.GameInterface.World.PlayerManager.LocalPlayerSpawned += 
            OnLocalPlayerSpawned;
        _hideTimer.Timeout += OnHideTimerTimeout;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        _craftingInterface.IngredientsContainer.IngredientIconMouseEntered +=
            OnIngredientIconMouseEntered;

        TreeExiting += () => {
            _craftingInterface.GameInterface.World.PlayerManager.LocalPlayerSpawned -=
                OnLocalPlayerSpawned;
            _hideTimer.Timeout -= OnHideTimerTimeout;
            MouseEntered -= OnMouseEntered;
            MouseExited -= OnMouseExited;
        };
    }

    private void OnIngredientIconMouseEntered(Control _, Ingredient __, string slotName) {
        _slotName = slotName;
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
        foreach (IngredientButton selectIngredientButton in _ingredientButtons) {
            selectIngredientButton.IngredientButtonClicked -= OnIngredientButtonClicked;
            selectIngredientButton.QueueFree();
        }

        _ingredientButtons.Clear();

        _player?.Inventory.StackedItemsList.ForEach(stackedItems => {
            if (!stackedItems.Item.HasProperty<ItemIngredient>()) return;
            IngredientButton newButton = IngredientButton.Create(stackedItems.Item);
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
        SelectIngredientButtonClicked?.Invoke(item, _slotName);
        Hide();
    }
}
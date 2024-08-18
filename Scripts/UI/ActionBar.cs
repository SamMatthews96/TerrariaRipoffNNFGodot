using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF.Scripts.UI;

public partial class ActionBar : PanelContainer {
    private ActionController _actionController;
    [Export] private PackedScene _actionBarButtonScene;
    [Export] private HBoxContainer _buttonContainer;

    public void Initialize(ActionController actionController) {
        _actionController = actionController;
        _actionController.Actions.ForEach(action => {
            ActionBarButton button = _actionBarButtonScene.Instantiate<ActionBarButton>();
            button.ButtonDown += () => OnActionButtonPressed(action);
            button.Initialize(action);
            _buttonContainer.AddChild(button);
        });
    }

    private void OnActionButtonPressed(IAction action) {
        GD.Print(nameof(action));
    }
}
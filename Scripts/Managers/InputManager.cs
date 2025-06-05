using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class InputManager : Node {
    private const string RunLeft = "runLeft";
    private const string RunRight = "runRight";
    private const string Jump = "jump";
    private const string LeftMouse = "leftMouse";
    private const string RightMouse = "rightMouse";
    private const string Save = "save";
    private const string ToggleInventory = "toggleInventory";
    private const string BuildMode = "buildMode";
    private const string GatherMode = "gatherMode";
    private const string GameMenu = "ingameMenu";
    private const string CraftMenu = "craftMenu";
    
    [Export] private Node2D _node2D;

    public event Action<Vector2> LeftMouseDown;
    public event Action<Vector2> LeftMouseUp;
    public event Action<Vector2> RightMouseDown;
    public event Action<Vector2> RightMouseUp;
    public event Action<int> HorizontalInputChanged;
    public event Action JumpPressed;
    public event Action SaveGamePressed;
    public event Action ToggleInventoryPressed;
    public event Action<PlayerActionType> PlayerActionModeChanged;
    public event Action EscapePressed;
    public event Action CraftMenuPressed;
    
    public override void _UnhandledInput(InputEvent e) {
        switch (e) {
            case InputEventMouseButton:
                HandleMouseEvents();
                break;
            case InputEventKey:
                HandleKeyEvents();
                break;
        }
    }

    private void HandleMouseEvents() {
        Vector2 mousePosition = _node2D.GetGlobalMousePosition();

        if (Input.IsActionJustPressed(LeftMouse)) {
            LeftMouseDown?.Invoke(mousePosition);
        }

        if (Input.IsActionJustReleased(LeftMouse)) {
            LeftMouseUp?.Invoke(mousePosition);
        }

        if (Input.IsActionJustPressed(RightMouse)) {
            RightMouseDown?.Invoke(mousePosition);
        }

        if (Input.IsActionJustReleased(RightMouse)) {
            RightMouseUp?.Invoke(mousePosition);
        }
    }

    private void HandleKeyEvents() {
        if (
            Input.IsActionJustPressed(RunLeft) ||
            Input.IsActionJustPressed(RunRight) ||
            Input.IsActionJustReleased(RunLeft) ||
            Input.IsActionJustReleased(RunRight)
        ) {
            bool isRunLeftPressed = Input.IsActionPressed(RunLeft);
            bool isRunRightPressed = Input.IsActionPressed(RunRight);

            if (isRunLeftPressed && !isRunRightPressed) {
                HorizontalInputChanged?.Invoke(-1);
            } else if (!isRunLeftPressed && isRunRightPressed) {
                HorizontalInputChanged?.Invoke(1);
            } else {
                HorizontalInputChanged?.Invoke(0);
            }
        }

        if (Input.IsActionJustPressed(Jump)) {
            JumpPressed?.Invoke();
        }

        if (Input.IsActionJustPressed(Save)) {
            SaveGamePressed?.Invoke();
        }

        if (Input.IsActionJustPressed(ToggleInventory)) {
            ToggleInventoryPressed?.Invoke();
        }

        if (Input.IsActionJustPressed(GatherMode)) {
            PlayerActionModeChanged?.Invoke(PlayerActionType.Gather);
        }

        if (Input.IsActionJustPressed(BuildMode)) {
            PlayerActionModeChanged?.Invoke(PlayerActionType.Build);
        }
        
        if (Input.IsActionJustPressed(GameMenu)) {
            EscapePressed?.Invoke();
        }
        
        if (Input.IsActionJustPressed(CraftMenu)) {
            CraftMenuPressed?.Invoke();
        }
        
        
        
    }
}
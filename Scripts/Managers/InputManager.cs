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

    [Export] private Node2D _node2D;

    public static InputManager Instance { get; private set; }


    public event Action<Vector2> LeftMouseDown;
    public event Action<Vector2> LeftMouseUp;
    public event Action<Vector2> RightMouseDown;
    public event Action<Vector2> RightMouseUp;
    public event Action<int> HorizontalInputChanged;
    public event Action JumpPressed;
    public event Action SaveGamePressed;
    public event Action ToggleInventoryPressed;

    public override void _EnterTree() {
        Instance = this;
    }

    public override void _Process(double delta) {
        // Vector2 mousePosition = GetViewport().GetMousePosition();
    }

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

        if (Godot.Input.IsActionJustPressed(LeftMouse)) {
            LeftMouseDown?.Invoke(mousePosition);
        }

        if (Godot.Input.IsActionJustReleased(LeftMouse)) {
            LeftMouseUp?.Invoke(mousePosition);
        }

        if (Godot.Input.IsActionJustPressed(RightMouse)) {
            RightMouseDown?.Invoke(mousePosition);
        }

        if (Godot.Input.IsActionJustReleased(RightMouse)) {
            RightMouseUp?.Invoke(mousePosition);
        }
    }

    private void HandleKeyEvents() {
        if (
            Godot.Input.IsActionJustPressed(RunLeft) ||
            Godot.Input.IsActionJustPressed(RunRight) ||
            Godot.Input.IsActionJustReleased(RunLeft) ||
            Godot.Input.IsActionJustReleased(RunRight)
        ) {
            bool isRunLeftPressed = Godot.Input.IsActionPressed(RunLeft);
            bool isRunRightPressed = Godot.Input.IsActionPressed(RunRight);

            if (isRunLeftPressed && !isRunRightPressed) {
                HorizontalInputChanged?.Invoke(-1);
            } else if (!isRunLeftPressed && isRunRightPressed) {
                HorizontalInputChanged?.Invoke(1);
            } else {
                HorizontalInputChanged?.Invoke(0);
            }
        }

        if (Godot.Input.IsActionJustPressed(Jump)) {
            JumpPressed?.Invoke();
        }

        if (Godot.Input.IsActionJustPressed(Save)) {
            SaveGamePressed?.Invoke();
        }

        if (Godot.Input.IsActionJustPressed(ToggleInventory)) {
            ToggleInventoryPressed?.Invoke();
        }
    }
}
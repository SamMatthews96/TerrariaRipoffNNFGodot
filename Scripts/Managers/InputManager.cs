using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Scripts.Managers;

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

    [Signal] public delegate void LeftMouseDownEventHandler(Vector2 mouseScreenPosition);

    [Signal] public delegate void LeftMouseUpEventHandler(Vector2 mouseScreenPosition);

    [Signal] public delegate void RightMouseDownEventHandler(Vector2 mouseScreenPosition);

    [Signal] public delegate void RightMouseUpEventHandler(Vector2 mouseScreenPosition);

    [Signal] public delegate void HorizontalInputChangedEventHandler(int horizontalInput);

    [Signal] public delegate void JumpPressedEventHandler();

    [Signal] public delegate void SaveGamePressedEventHandler();

    [Signal] public delegate void ToggleInventoryPressedEventHandler();

    public override void _EnterTree() {
        Instance = this;
    }

    public override void _Process(double delta) {
        if (
            Input.IsActionJustPressed(RunLeft) ||
            Input.IsActionJustPressed(RunRight) ||
            Input.IsActionJustReleased(RunLeft) ||
            Input.IsActionJustReleased(RunRight)
        ) {
            bool isRunLeftPressed = Input.IsActionPressed(RunLeft);
            bool isRunRightPressed = Input.IsActionPressed(RunRight);

            if (isRunLeftPressed && !isRunRightPressed) {
                EmitSignal(SignalName.HorizontalInputChanged, -1);
            } else if (!isRunLeftPressed && isRunRightPressed) {
                EmitSignal(SignalName.HorizontalInputChanged, 1);
            } else {
                EmitSignal(SignalName.HorizontalInputChanged, 0);
            }
        }

        if (Input.IsActionJustPressed(Jump)) {
            EmitSignal(SignalName.JumpPressed);
        }

        if (Input.IsActionJustPressed(Save)) {
            EmitSignal(SignalName.SaveGamePressed);
        }

        // Vector2 mousePosition = GetViewport().GetMousePosition();
        Vector2 mousePosition = _node2D.GetGlobalMousePosition();

        if (Input.IsActionJustPressed(LeftMouse)) {
            EmitSignal(SignalName.LeftMouseDown, mousePosition);
        }

        if (Input.IsActionJustReleased(LeftMouse)) {
            EmitSignal(SignalName.LeftMouseUp, mousePosition);
        }

        if (Input.IsActionJustPressed(RightMouse)) {
            EmitSignal(SignalName.RightMouseDown, mousePosition);
        }

        if (Input.IsActionJustReleased(RightMouse)) {
            EmitSignal(SignalName.RightMouseUp, mousePosition);
        }

        if (Input.IsActionJustPressed(ToggleInventory)) {
            EmitSignal(SignalName.ToggleInventoryPressed);
        }
    }
}
using Godot;
using System;

public partial class InputTest : Node {
    private const string RUN_LEFT = "runLeft";
    private const string RUN_RIGHT = "runRight";

    [Signal]
    public delegate void HorizontalInputChangedEventHandler(int horizontalInput);

    public static InputTest Instance { get; private set; }


    public override void _Ready() {
        Instance = this;
    }

    public override void _Process(double delta) {
        if (
            Input.IsActionJustPressed(RUN_LEFT) ||
            Input.IsActionJustPressed(RUN_RIGHT) ||
            Input.IsActionJustReleased(RUN_LEFT) ||
            Input.IsActionJustReleased(RUN_RIGHT)
        ) {
            SetHorizontalInput();
        }
    }

    private void SetHorizontalInput() {
        bool isRunLeftPressed = Input.IsActionPressed(RUN_LEFT);
        bool isRunRightPressed = Input.IsActionPressed(RUN_RIGHT);

        if (isRunLeftPressed && !isRunRightPressed) {
            EmitSignal(SignalName.HorizontalInputChanged, -1);
        } else if (!isRunLeftPressed && isRunRightPressed) {
            EmitSignal(SignalName.HorizontalInputChanged, 1);
        } else {
            EmitSignal(SignalName.HorizontalInputChanged, 0);
        }

    }
}
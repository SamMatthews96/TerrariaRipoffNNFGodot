using Godot;

namespace TerrariaRipoffNNF.Scripts.Managers;

public partial class InputManager : Node {
    private const string RunLeft = "runLeft";
    private const string RunRight = "runRight";
    private const string Jump = "jump";
    private const string LeftMouse = "leftMouse";
    private const string Save = "save";
    private const string ToggleInventory = "toggleInventory";

    [Signal] public delegate void HorizontalInputChangedEventHandler(int horizontalInput);

    [Signal] public delegate void JumpPressedEventHandler();

    [Signal] public delegate void MouseClickedEventHandler(Vector2 mouseScreenPosition);

    [Signal] public delegate void SaveGamePressedEventHandler();

    [Signal] public delegate void ToggleInventoryPressedEventHandler();

    public static InputManager Instance { get; private set; }

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

        if (Input.IsActionJustReleased(LeftMouse)) {
            Vector2 mousePosition = GetViewport().GetMousePosition();
            EmitSignal(SignalName.MouseClicked, mousePosition);
        }

        if (Input.IsActionJustPressed(ToggleInventory)) {
            EmitSignal(SignalName.ToggleInventoryPressed);
        }
    }
}
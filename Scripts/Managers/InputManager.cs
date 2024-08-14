using Godot;

namespace TerrariaRipoffNNF.Scripts.Managers;

public partial class InputManager : Node {
    private const string RUN_LEFT = "runLeft";
    private const string RUN_RIGHT = "runRight";
    private const string JUMP = "jump";
    private const string LEFT_MOUSE = "leftMouse";
    private const string SAVE = "save";

    [Signal]
    public delegate void HorizontalInputChangedEventHandler(int horizontalInput);

    [Signal]
    public delegate void JumpPressedEventHandler();

    [Signal]
    public delegate void MouseClickedEventHandler(Vector2 mouseScreenPosition);

    [Signal]
    public delegate void SaveGamePressedEventHandler();

    public static InputManager Instance { get; private set; }

    public override void _EnterTree() {
        Instance = this;
    }

    public override void _Process(double delta) {
        if (
            Input.IsActionJustPressed(RUN_LEFT) ||
            Input.IsActionJustPressed(RUN_RIGHT) ||
            Input.IsActionJustReleased(RUN_LEFT) ||
            Input.IsActionJustReleased(RUN_RIGHT)
        ) {
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

        if (Input.IsActionJustPressed(JUMP)) {
            EmitSignal(SignalName.JumpPressed);
        }

        if (Input.IsActionJustPressed(SAVE)) {
            EmitSignal(SignalName.SaveGamePressed);
        }

        if (Input.IsActionJustReleased(LEFT_MOUSE)) {
            Vector2 mousePosition = GetViewport().GetMousePosition();
            EmitSignal(SignalName.MouseClicked, mousePosition);
        }
    }
}
using Godot;

namespace TerrariaRipoffNNF;

public partial class SelectGameTypeMenu : Control {
    [Export] public Button SinglePlayerButton { get; private set; }
    [Export] public Button MultiplayerButton { get; private set; }
    [Export] public Button ExitButton { get; private set; }
}
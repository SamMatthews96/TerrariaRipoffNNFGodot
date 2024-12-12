using Godot;

namespace TerrariaRipoffNNF;

public partial class MultiplayerMenu : Control {
    [Export] public Button HostButton { get; private set; }
    [Export] public Button JoinButton { get; private set; }
    [Export] public Button BackButton { get; private set; }
}
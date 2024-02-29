using System;
using Godot;

namespace TerrariaRipoffNNF.Scenes.Scripts;

public partial class Player : CharacterBody2D {
    public static Player LocalPlayer { get; private set; }

    [Export] private MultiplayerSynchronizer positionSynchronizer;
    [Export] private float speed = 300f;

    private int XCoords => (int)Math.Round(Position.X / WorldManager.Instance.BlockSize);
    private int YCoords => (int)Math.Round(Position.Y / WorldManager.Instance.BlockSize);

    [Signal]
    public delegate void LocalPlayerMovedEventHandler(
        int xCoords, int yCoords, int prevXCoords, int prevYCoords);

    private int horizontalInput;

    public override void _EnterTree() {
        int peerId = Name.ToString()!.ToInt();
        positionSynchronizer.SetMultiplayerAuthority(peerId);
        if (peerId != Multiplayer.GetUniqueId()) return;

        LocalPlayer = this;
        InputManager.Instance.HorizontalInputChanged += newInput => horizontalInput = newInput;
        PlayerManager.Instance.CreatedLocalPlayer += (xSpawnCoords, ySpawnCoords) => {
            int blockSize = WorldManager.Instance.BlockSize;
            Position = new Vector2(xSpawnCoords * blockSize, ySpawnCoords * blockSize);
        };
    }

    public override void _Process(double delta) {
        if (this != LocalPlayer) return;

        (int previousXCoords, int previousYCoords) = (XCoords, YCoords);
        Velocity = new Vector2(speed * horizontalInput, speed);
        MoveAndSlide();
        if (previousXCoords != XCoords || previousYCoords != YCoords) {
            EmitSignal(SignalName.LocalPlayerMoved,
                XCoords, YCoords, previousXCoords, previousYCoords);
        }
         
    }
}
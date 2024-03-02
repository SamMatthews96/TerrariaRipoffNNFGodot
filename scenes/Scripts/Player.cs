using System;
using Godot;
using TerrariaRipoffNNF.Managers.Scripts;

namespace TerrariaRipoffNNF.Scenes.Scripts;

public partial class Player : CharacterBody2D {
    [Export] private float speed = 300f;
    [Export] private MultiplayerSynchronizer positionSynchronizer;
    private int horizontalInput;

    [Signal]
    public delegate void LocalPlayerMovedEventHandler(
        int xCoords, int yCoords, int prevXCoords, int prevYCoords);

    public static Player LocalPlayer { get; private set; }

    private int XCoords => (int)Math.Round(Position.X / WorldManager.BLOCK_SIZE);
    private int YCoords => (int)Math.Round(Position.Y / WorldManager.BLOCK_SIZE);

    public override void _EnterTree() {
        int peerId = Name.ToString()!.ToInt();
        positionSynchronizer.SetMultiplayerAuthority(peerId);
        if (peerId != Multiplayer.GetUniqueId()) return;

        LocalPlayer = this;
        InputManager.Instance.HorizontalInputChanged += newInput => horizontalInput = newInput;
        PlayerManager.Instance.CreatedLocalPlayer += (xSpawnCoords, ySpawnCoords) => {
            Position = new Vector2(
                xSpawnCoords * WorldManager.BLOCK_SIZE, ySpawnCoords * WorldManager.BLOCK_SIZE);
        };
    }

    public override void _PhysicsProcess(double delta) {
        if (this != LocalPlayer) return;

        (int previousXCoords, int previousYCoords) = (XCoords, YCoords);
        Velocity = new Vector2(speed * horizontalInput, speed);
        MoveAndSlide();
        if (previousXCoords != XCoords || previousYCoords != YCoords) {
            EmitSignal(SignalName.LocalPlayerMoved,
                XCoords, YCoords, previousXCoords, previousYCoords);
        }
    }
    
    /*
     * states
     *      falling
     *      grounded
     *      swimming
     *      
     */
}
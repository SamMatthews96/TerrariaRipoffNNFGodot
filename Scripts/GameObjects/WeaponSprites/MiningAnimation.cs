using System;
using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

public partial class MiningAnimation : Node2D {
    [Export] private Timer _timer;
    private float _startRotation = -(float)Math.PI / 4;
    private float _rotationSpeed = (float)Math.PI * 2;
    private float _rotationAmount = (float)Math.PI / 2;

    public static MiningAnimation Create() {
        return Data.PackedScenes.PickaxeSwing.Instantiate<MiningAnimation>();
    }
    
    public override void _Ready() {
        Rotation = _startRotation;
        _timer.WaitTime = _rotationAmount / _rotationSpeed;
        _timer.Start();
        _timer.Timeout += OnTimeout;
    }

    private void OnTimeout() {
        _timer.Timeout -= OnTimeout;
        QueueFree();
    }

    public override void _Process(double delta) {
        Rotation += _rotationSpeed * (float)delta;   
    }
    
}
using System;
using Godot;

namespace TerrariaRipoffNNF.Scripts.Interface;

public partial class LoadingScreen : Node {
    [Export] private Sprite2D _loadingSprite;
    [Export] private Label _loadingText;
    [Export] private Timer _loadingTimer;
    private int _timeoutCount;
    
    public override void _Ready() {
        _loadingTimer.Timeout += OnTimeOut;
        TreeExiting += () => {
            _loadingTimer.Timeout -= OnTimeOut;
        };
    }

    private void OnTimeOut() {
        _timeoutCount++;
        float timeSpent = 
            (float)Math.Round(_timeoutCount * _loadingTimer.WaitTime, 1);
        _loadingText.Text = $"Loading... {timeSpent}s";
    }

    public override void _Process(double delta) {
        _loadingSprite.RotationDegrees += (float)delta * 120;
    }
}
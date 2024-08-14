using System;
using Godot;

namespace TerrariaRipoffNNF.testScripts;

public partial class FpsCounter : Label {
    private double _lowestFps = double.MaxValue;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        double fps = Math.Round(1 / delta);
        _lowestFps = Math.Min(_lowestFps, fps);
        Text = "FPS: " + fps + "\nLowest FPS: " + _lowestFps;
        
        if (Input.IsActionJustPressed("resetLowestFps")) {
            ResetLowestFps();
        }
    }

    private void ResetLowestFps() {
        _lowestFps = double.MaxValue;
    }
}
using System;
using Godot;

namespace TerrariaRipoffNNF.testScripts;

public partial class FpsCounter : Label {
    private double _lowestFps = double.MaxValue;

    public override void _Ready() { }

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
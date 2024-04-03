using Godot;
using System;

public partial class Test : Node {
    private Random random;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        GD.Print("1");

    }

    public override void _Process(double delta) { }
}
using Godot;
using System;
using System.Diagnostics;
using TerrariaRipoffNNF.Resources.Scripts;

public partial class Test : Node {
	private Random random;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		random = new();
	}

	public override void _Process(double delta) {
		GD.Print(random.Next(2));
	}
}

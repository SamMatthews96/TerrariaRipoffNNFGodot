using System;
using Godot;
using static Godot.GD;

namespace TerrariaRipoffNNF.scripts;

public partial class Player : CharacterBody2D {
	
	[Export] private MultiplayerSynchronizer multiplayerSynchronizer;
	[Export] private float speed = 300f;

	private int horizontalInput;
	
	public override void _Ready() {
	}

	public override void _Process(double delta) {
		Position += new Vector2((float)delta * speed * horizontalInput, 0);
	}

	

	
	
	
}

using System;
using Godot;
using static Godot.GD;

namespace TerrariaRipoffNNF.scripts;

public partial class Player : CharacterBody2D {
	
	[Export] private MultiplayerSynchronizer multiplayerSynchronizer;
	[Export] private float speed = 300f;

	private int horizontalInput = 0;
	
	public static Player LocalPlayer { get; private set; }

	public override void _Ready() {
		multiplayerSynchronizer.SetMultiplayerAuthority(int.Parse(Name));
		if (Multiplayer.GetUniqueId() == int.Parse(Name)) {
			LocalPlayer = this;
			InputTest.Instance.HorizontalInputChanged += input => horizontalInput = input;
		}
	}

	public override void _Process(double delta) {
		Position += new Vector2((float)delta * speed * horizontalInput, 0);
	}

	

	
	
	
}

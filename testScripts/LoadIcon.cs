using System;
using System.Globalization;
using Godot;

namespace TerrariaRipoffNNF.testScripts; 

public partial class LoadIcon : Node2D {
	[Export] private Label label;
	[Export] private Sprite2D sprite;
	public override void _Process(double delta) {
		sprite.Rotation += (float)delta * 3.14159f;
		label.Text = Math.Round(1 / delta).ToString(CultureInfo.InvariantCulture);
	}
}
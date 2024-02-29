using Godot;

namespace TerrariaRipoffNNF.Scenes.Scripts; 

public partial class ActiveBlock : Node2D {

	[Export] private Sprite2D sprite;
	public TerrariaRipoffNNF.Resources.Scripts.BlockType BlockType { get; set; }

	public override void _Ready() {
		sprite.Texture = BlockType.Texture;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
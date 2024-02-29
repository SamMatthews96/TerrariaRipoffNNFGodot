using System;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Scenes.Scripts; 

public partial class ActiveBlock : Node2D {
	[Export] private static PackedScene packedActiveBlock = 
		ResourceLoader.Load<PackedScene>("res://Scenes/ActiveBlock.tscn");

	public static ActiveBlock Instantiate(BlockType blockType, Vector2 position) {
		ActiveBlock newBlock = packedActiveBlock.Instantiate<ActiveBlock>();
		newBlock.Position = position;
		newBlock.blockType = blockType;
		newBlock.sprite.Texture = blockType.Texture;
		return newBlock;
	}

	[Export] private Sprite2D sprite;
	private BlockType blockType;
	
}
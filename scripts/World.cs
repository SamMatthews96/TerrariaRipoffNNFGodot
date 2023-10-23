using System;
using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.scripts.BlockScripts;
using static Godot.GD;

namespace TerrariaRipoffNNF.scripts;

public partial class World : Node2D {
	[Export] public Node2D PlayersContainer;
	[Export] public CanvasLayer CanvasLayer;
	public static World Instance { get; private set; }

	private const int BLOCK_OBJECT_DISTANCE = 10;
	public const int BLOCK_SIZE = 50;

	private PackedScene blockPackedScene = Load<PackedScene>("res://scenes/block.tscn");
	private PackedScene playerPackedScene = Load<PackedScene>("res://scenes/player.tscn");

	private List<Player> players = new();
	
	public override void _Ready() {
		Instance = this;
		Block.OnStaticCreated += Block_OnStaticCreated;
		Player.OnPlayerSpawned += Player_OnPlayerSpawned;
	}
	
	private void OnBlockTickDamage() {
		for (int x = 0; x < Block.WORLD_WIDTH; x++) {
			for (int y = 0; y < Block.WORLD_HEIGHT; y++) {
				Block block = Block.GetBlockAtPosition(x, y);
				if (block is null) continue;
				if (block.Stability.ExcessBurden == 0) continue;
				
				block.TakeDamage(block.Stability.ExcessBurden);
			}
		}
	}

	private void Block_OnStaticCreated(object sender, EventArgs _) {
		Block block = (Block)sender;
		
		foreach (Player player in players) {
			(int playerXPosition, int playerYPosition) = player.GetCellPosition();
			if (Math.Abs(block.XPosition - playerXPosition) >= BLOCK_OBJECT_DISTANCE) continue;
			if (Math.Abs(block.YPosition - playerYPosition) >= BLOCK_OBJECT_DISTANCE) continue;
			block.EnableBlockObject();
			break;
		}
	}

	private void Player_OnPlayerSpawned(object sender, EventArgs _) {
		Player player = (Player)sender;
		SpawnLocalBlocks(player);
		player.OnPlayerMovedCell += Player_OnPlayerMovedCell;
		players.Add(player);
	}

	private void Player_OnPlayerMovedCell(object sender, Player.OnPlayerMovedCellEventArgs eventArgs) {
		Player player = (Player)sender;
		SpawnLocalBlocks(player);
	}

	private void SpawnLocalBlocks(Player player) {
		(int playerXCell, int playerYCell) = player.GetCellPosition();

		int xStartPosition = Math.Max(0, playerXCell - BLOCK_OBJECT_DISTANCE);
		int yStartPosition = Math.Max(0, playerYCell - BLOCK_OBJECT_DISTANCE);
		int xEndPosition = Math.Min(Block.WORLD_WIDTH, playerXCell + BLOCK_OBJECT_DISTANCE);
		int yEndPosition = Math.Min(Block.WORLD_HEIGHT, playerYCell + BLOCK_OBJECT_DISTANCE);

		List<Block> closeBlocks = Block.GetBlocksInArea(
			xStartPosition, yStartPosition, xEndPosition, yEndPosition);
		foreach (Block block in closeBlocks) {
			block.EnableBlockObject();
		}
	}

	private void SpawnPlayer() {
		var player = playerPackedScene.Instantiate<Player>();
		player.Position = new Vector2(50, -199);
		AddChild(player);
	}

	public BlockNode CreateBlockObject(Block block) {
		var instance = blockPackedScene.Instantiate<BlockNode>();
		instance.Block = block;
		instance.Position = new Vector2(block.XPosition * BLOCK_SIZE, -block.YPosition * BLOCK_SIZE);
		AddChild(instance);
		return instance;
	}
}

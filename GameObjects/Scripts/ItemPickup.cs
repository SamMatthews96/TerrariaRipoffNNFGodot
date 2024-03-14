using Godot;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Scenes.Scripts; 

public partial class ItemPickup : Node2D {
	private InventoryItemType _inventoryItemType;
	[Export] private Sprite2D sprite;
	private int _xPosition;
	private int _yPosition;
	
	public void Initialize(InventoryItemType inventoryItemType, int xPosition, int yPosition) {
		_inventoryItemType = inventoryItemType;
		_xPosition = xPosition;
		_yPosition = yPosition;
		sprite.Texture = inventoryItemType.IconTexture;
		Position = Utils.GetWorldPositionFromCellCoordinates(_xPosition, _yPosition);
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
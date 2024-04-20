using Godot;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.GameObjects.Scripts; 

public partial class ItemPickup : Node2D {
	[Export] private Sprite2D sprite;
	private int _xPosition;
	private int _yPosition;
	private InventoryItemType _inventoryItemType;	
	
	public void Initialize(InventoryItemType inventoryItemType, int xPosition, int yPosition) {
		_inventoryItemType = inventoryItemType;
		_xPosition = xPosition;
		_yPosition = yPosition;
		Position = WorldManager.Instance.GetWorldPositionFromCellCoordinates(_xPosition, _yPosition);
	}

	public override void _Ready() {
		sprite.Texture = _inventoryItemType.IconTexture;
	}
}

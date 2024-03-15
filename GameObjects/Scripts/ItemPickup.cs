using Godot;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.GameObjects.Scripts; 

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
}

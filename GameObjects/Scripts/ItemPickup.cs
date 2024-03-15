using Godot;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.GameObjects.Scripts; 

public partial class ItemPickup : Node2D {
	[Export] private Sprite2D sprite;
	private int _xPosition;
	private int _yPosition;
	[Export] private string _resourcePath;
	private InventoryItemType _inventoryItemType;	
	
	public void Initialize(string resourcePath, int xPosition, int yPosition) {
		_resourcePath = resourcePath;
		_xPosition = xPosition;
		_yPosition = yPosition;
		Position = Utils.GetWorldPositionFromCellCoordinates(_xPosition, _yPosition);
	}

	public override void _Ready() {
		_inventoryItemType = InventoryItemType.Deserialize(_resourcePath);
		sprite.Texture = _inventoryItemType.IconTexture;
	}
}

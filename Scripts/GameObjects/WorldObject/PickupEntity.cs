using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF;

public partial class PickupEntity : Node2D, IEntity {
    [Export] private Sprite2D _pickupSprite;
    private Item _item;
    public Vector2I CellCoordinates;
    
}
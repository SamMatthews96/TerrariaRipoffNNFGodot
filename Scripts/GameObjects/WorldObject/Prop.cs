using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Prop : Node2D {
    private Item _item;
    private Array<Vector2I> _cells;

    public override void _Ready() {
        ItemProp itemProp = _item.GetProperty<ItemProp>();
        
    }
}
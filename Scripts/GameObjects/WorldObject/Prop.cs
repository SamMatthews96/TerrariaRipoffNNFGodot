using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Prop : Node2D {
    public Item Item { get; private set; }
    public Array<Vector2I> Cells { get; private set; } = new();
    [Export] private Sprite2D _sprite;

    public static Prop Create(Item item, Vector2I coords) {
        Prop newProp = Data.PackedScenes.Prop.Instantiate<Prop>();
        newProp.Position = coords * Game.BlockSize;
        newProp.Item = item;
        ItemProp itemProp = item.GetProperty<ItemProp>();
        // let coords be the top left of the prop
        for (int x = 0; x < itemProp.Dimensions.X; x++) {
            for (int y = 0; y < itemProp.Dimensions.Y; y++) {
                newProp.Cells.Add(coords + new Vector2I(x, y));
            }
        }

        newProp._sprite.Texture = itemProp.Texture;
        
        return newProp;
    }
  
}
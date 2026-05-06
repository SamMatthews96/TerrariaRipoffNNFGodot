using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class ActiveProp : Node2D {
    public Item Item { get; private set; }
    public Array<Vector2I> Cells { get; private set; } = new();
    public Vector2I Anchor { get; private set; }
    [Export] private Sprite2D _sprite;
    
    public static ActiveProp Create(Item item, Vector2I coords) {
        ActiveProp newActiveProp = Data.PackedScenes.Prop.Instantiate<ActiveProp>();
        newActiveProp.Anchor = coords;
        newActiveProp.Position = coords * Game.BlockSize;
        newActiveProp.Item = item;
        ItemProp itemProp = item.GetProperty<ItemProp>();
        for (int x = 0; x < itemProp.Dimensions.X; x++) {
            for (int y = 0; y < itemProp.Dimensions.Y; y++) {
                newActiveProp.Cells.Add(coords + new Vector2I(x, y));
            }
        }

        newActiveProp._sprite.Texture = itemProp.Texture;
        
        return newActiveProp;
    }
}
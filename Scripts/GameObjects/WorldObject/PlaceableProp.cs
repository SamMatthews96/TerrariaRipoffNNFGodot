using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlaceableProp : Prop {
    public static PlaceableProp Create(Item item, Vector2I coords) {
        PlaceableProp newProp = Data.PackedScenes.Prop.Instantiate<PlaceableProp>();
        newProp.Position = coords * Game.BlockSize;
        newProp.Item = item;
        ItemProp itemProp = item.GetProperty<ItemProp>();
        // let coords be the top left of the prop
        for (int x = 0; x < itemProp.Dimensions.X; x++) {
            for (int y = 0; y < itemProp.Dimensions.Y; y++) {
                newProp.Cells.Add(coords + new Vector2I(x, y));
            }
        }

        newProp.Sprite.Texture = itemProp.Texture;
        
        return newProp;
    }
  
}
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class BreakableProp : Prop {
    public Breakable Breakable { get; private set; }
    public static BreakableProp Create(Breakable breakable, Vector2I coords) {
        BreakableProp newProp = Data.PackedScenes.Breakable.Instantiate<BreakableProp>();
        newProp.Breakable = breakable;
        newProp.Position = coords * Game.BlockSize;
        newProp.Item = breakable.Item;
        // let coords be the top left of the prop
        for (int x = 0; x < breakable.Dimensions.X; x++) {
            for (int y = 0; y < breakable.Dimensions.Y; y++) {
                newProp.Cells.Add(coords + new Vector2I(x, y));
            }
        }

        newProp.Sprite.Texture = breakable.Texture;
        
        return newProp;
    }
}
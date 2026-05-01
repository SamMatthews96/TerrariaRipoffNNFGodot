using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class BreakableProp : Node2D {
    public Breakable Breakable { get; private set; }
    public Array<Vector2I> Cells { get; private set; } = new();

    [Export] private Sprite2D _sprite;
    
    public static BreakableProp Create(Breakable breakable, Vector2I coords) {
        BreakableProp newProp = Data.PackedScenes.Breakable.Instantiate<BreakableProp>();
        newProp.Position = coords * Game.BlockSize;
        // let coords be the top left of the prop
        for (int x = 0; x < breakable.Dimensions.X; x++) {
            for (int y = 0; y < breakable.Dimensions.Y; y++) {
                newProp.Cells.Add(coords + new Vector2I(x, y));
            }
        }

        newProp._sprite.Texture = breakable.Texture;
        
        return newProp;
    }
}
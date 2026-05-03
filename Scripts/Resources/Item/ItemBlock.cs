using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemBlock : ItemProperty {
    [Export] public Texture2D Texture { get; private set; }
    
    public override Dictionary GetTooltipAttributes() {
        return new Dictionary();
    }

    public ItemBlock(Texture2D texture) { 
        Texture = texture;
    }

    public ItemBlock() { }
}
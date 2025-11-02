using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipeFieldMapTextureConstant : RecipeFieldMapTexture {
    [Export] private Texture2D _value;
    public override Texture2D ResolveTemplate(Dictionary<string, Item> _) {
        return _value;
    }
}
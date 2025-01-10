using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class ItemPlaceable : ItemProperty {
    public override PropertyType PropertyType => PropertyType.Placeable;

    public override Dictionary ToDictionary() {
        throw new System.NotImplementedException();
    }

    public override Dictionary GetTooltipAttributes() {
        throw new System.NotImplementedException();
    }
}
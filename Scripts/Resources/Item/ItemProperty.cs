using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class ItemProperty : Resource {
    public abstract Dictionary GetTooltipAttributes();
}
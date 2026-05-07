using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ActivePrinter : ActiveActor {
    public override void Action() {
        GD.Print("Bang");
    }
}
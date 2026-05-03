using Godot;

namespace TerrariaRipoffNNF.TestScenes.Tools;

[Tool]
public partial class PrintTest : EditorScript {
    public override void _Run() {
        GD.Print("Hello World!");
    }
}
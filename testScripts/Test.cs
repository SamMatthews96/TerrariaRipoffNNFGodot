using Godot;
using Godot.Collections;
using TerrariaRipoffNNF;

public partial class Test : Node
{
    public override void _Ready() {
        Dictionary myDic = new() {
            {"key1", "val1"}
        };
        GD.Print(myDic.ToString());
    }

    
}

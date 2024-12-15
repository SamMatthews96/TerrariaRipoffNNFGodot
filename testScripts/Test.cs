using Godot;

namespace TerrariaRipoffNNF;

public partial class Test : Node {
    
    private enum TestEnum {
        Test1,
        Test2,
        Test3
    }
    public override void _Ready() {
        TestEnum test;
        test = TestEnum.Test1;
        GD.Print(test);
        GD.Print(test.ToString());
        GD.Print((int)test);
    } 
}
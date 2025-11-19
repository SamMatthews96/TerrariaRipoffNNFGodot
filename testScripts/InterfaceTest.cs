using Godot;

namespace TerrariaRipoffNNF;

public partial class InterfaceTest : Node, ITest {
    private void Test() {
        GetTree();
    }
}

public interface ITest {
    public SceneTree GetTree();
}

public class Test {
    private void AnotherTest() {
        InterfaceTest test = new();
        ITest test2 = test;
        test2.GetTree();
    }
}
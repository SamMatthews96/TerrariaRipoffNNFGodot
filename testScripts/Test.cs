using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.testScripts;

public partial class Test : Node {
    
    public class Builder {
        private Test _test;
        private Node _parent;
        
        public Builder(Node parent, PackedScene packedScene) {
            _test = packedScene.Instantiate<Test>();
            _parent = parent;
        }

        public Test Build() {
            _parent.AddChild(_test);
            return _test;
        }
        
    }

    public static Builder New(Node parent, PackedScene packedScene) {
        return new Builder(parent, packedScene);
    }
}

public class MyBuilder {
    [Export] private PackedScene _packedScene;
    [Export] private Node _parent;
    private void Func() {
        Test.New(_parent, _packedScene).Build();
    }
}
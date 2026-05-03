using Godot;

namespace TerrariaRipoffNNF.TestScenes.ResourceChange;

[GlobalClass][Tool]
public partial class TestResource : Resource {
    [Export] private string TestString {
        get => _testString;
        set {
            GD.Print("change");
            _testString = value;
            _target = value;
        }
    }
    private string _testString;

    [Export] private string _target;
}
using Godot;
using System;

public partial class Test : Node
{
    public override void _Ready() {
        Resource test = new Resource();
        test.ResourceName = "TestResource";
        GD.Print(test.ResourceName);
        Rpc(nameof(TestVariantPassing), test);
    }

    [Rpc]
    private void TestVariantPassing(Resource resource) {
        GD.Print(resource.ResourceName);
    }
}

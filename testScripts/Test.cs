using Godot;

namespace TerrariaRipoffNNF;

public partial class Test : Node {
    
    private enum TestEnum {
        Test1,
        Test2,
        Test3
    }
    public override void _Ready() {
        Item res = Item.New()
            .SetName("test")
            .SetInventorySpace(4)
            .AddProperty(
                ItemMining.New()
                    .SetRange(4)
                    .Build()
                )
            .Build();
        
        ResourceSaver.Save(res, "res://test.tres");
        
        Item item = (Item)ResourceLoader.Load("res://test.tres");
        GD.Print(item.GetProperty<ItemMining>().Range);
    } 
}
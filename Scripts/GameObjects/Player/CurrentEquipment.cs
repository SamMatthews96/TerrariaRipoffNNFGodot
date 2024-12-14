using Godot;

namespace TerrariaRipoffNNF;

public partial class CurrentEquipment : Node {
    [Export] private Player _player;

    public ItemEquipment Pickaxe { get; private set; }


    public override void _Ready() {
        // @todo temporary
        // ItemEquipment pickaxe = ItemEquipment.New(
        //     ItemMining.New(1, 8, 10)
        //         .Build()
        // ).Build();
        //
        // if (pickaxe.HasProperty<ItemMining>()) {
        //     Pickaxe = pickaxe;
        // }
    }
}
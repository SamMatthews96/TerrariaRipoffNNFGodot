using Godot;

namespace TerrariaRipoffNNF;

public partial class CurrentEquipment : Node {
    [Export] private Player _player;

    public Equipment Pickaxe { get; private set; }


    public override void _Ready() {
        Equipment pickaxe = Equipment.New(
            MiningSlot.New(1, 8, 10)
                .Build()
        ).Build();

        if (pickaxe.HasProperty<MiningSlot>()) {
            Pickaxe = pickaxe;
        }
    }
}
using Godot;

namespace TerrariaRipoffNNF;

public partial class PlayerEquipment : Node {
    [Export] private Player _player;

    public ItemEquipment Pickaxe { get; private set; }


    public override void _Ready() {
    }
}
using Godot;

namespace TerrariaRipoffNNF;

public partial class Equipment : Node {
    [Export] private Player _player;
    
    public Item Pickaxe { get; private set; }
    public Item Hammer { get; private set; }
     
    public override void _Ready() {
        // @todo equip pickaxe and hammer based on playerDictionary
    }
}
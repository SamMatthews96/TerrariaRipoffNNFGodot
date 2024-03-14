using Godot;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Scenes.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class ItemPickupManager : Node {
    [Export] private PackedScene _itemPickupPackedScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    private void OnWorldManagerDeletedActiveBlock(BlockType blockType, int xPosition, int yPosition) {
        ItemPickup itemPickup = _itemPickupPackedScene.Instantiate<ItemPickup>();
        itemPickup.Initialize(blockType, xPosition, yPosition);
    }
}
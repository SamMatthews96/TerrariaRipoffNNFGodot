using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;
using TerrariaRipoffNNF.Scripts.UI;

namespace TerrariaRipoffNNF.Scripts.Managers;

public partial class UiManager : CanvasLayer {
    [Export] private InventoryUi _inventoryUi;
    public void Initialize(Player player) {
        _inventoryUi.Initialize(player);
    }
}
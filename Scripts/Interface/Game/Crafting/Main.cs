using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class Main : Control {
    [Export] private Button _selectCraftingStationMenuButton;

    [Export] public StationContainer CraftStationContainer { get; private set; }
    [Export] public SelectRecipeContainer SelectRecipeContainer { get; private set; }
    [Export] public SelectedRecipeContainer SelectedRecipeContainer { get; private set; }

    public override void _Ready() {
        Hide();
        Player.BeforeLocalPlayerSpawned += OnLocalPlayerSpawned;
        Manager.Instance.Game.InputManager.CraftMenuPressed += OnCraftMenuPressed;
    }

    public override void _ExitTree() {
        Player.BeforeLocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    private void OnLocalPlayerSpawned(Player player) {
        player.Crafting.CraftingStationAdded += OnCraftingStationAdded;
    }

    private void OnCraftingStationAdded(CraftingStation craftingStation) { }

    private void OnCraftMenuPressed() {
        if (Visible) {
            Hide();
        } else {
            Show();
        }
    }
}
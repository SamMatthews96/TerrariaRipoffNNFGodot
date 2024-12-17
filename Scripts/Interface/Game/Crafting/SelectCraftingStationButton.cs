using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class SelectCraftingStationButton : TextureButton {
    public CraftingStation _craftingStation;
   
    public event Action<CraftingStation> CraftingStationButtonClicked;
    
    public static SelectCraftingStationButton Create(CraftingStation craftingStation) {
        SelectCraftingStationButton button =
            Manager.Instance.PackedScenes.SelectCraftingStationButton
                .Instantiate<SelectCraftingStationButton>();
        button._craftingStation = craftingStation;
        // button.TextureNormal = craftingStation.Icon;
        return button;
    }

    public override void _Ready() {
        ButtonDown += OnButtonDown;
    }
    
    public override void _ExitTree() {
        ButtonDown -= OnButtonDown;
    }
    
    private void OnButtonDown() {
        CraftingStationButtonClicked?.Invoke(_craftingStation);
    }
}
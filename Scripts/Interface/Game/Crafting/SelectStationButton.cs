using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectStationButton : TextureButton {
    private CraftingStation _craftingStation;
   
    public event Action<CraftingStation> CraftingStationButtonClicked;
    
    public static SelectStationButton Create(CraftingStation craftingStation) {
        SelectStationButton button =
            Data.PackedScenes.SelectCraftingStationButton
                .Instantiate<SelectStationButton>();
        button._craftingStation = craftingStation;
        button.TextureNormal = craftingStation.Icon;
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
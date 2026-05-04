using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class CraftStationButton : TextureButton {
    private CraftingStation _craftingStation;
   
    public event Action<CraftingStation> StationButtonClicked;
    
    public static CraftStationButton Create(CraftingStation craftingStation) {
        CraftStationButton button =
            Data.PackedScenes.SelectCraftingStationButton
                .Instantiate<CraftStationButton>();
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
        StationButtonClicked?.Invoke(_craftingStation);
    }
}
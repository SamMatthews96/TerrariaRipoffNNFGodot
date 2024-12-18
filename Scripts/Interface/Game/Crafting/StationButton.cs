using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class StationButton : TextureButton {
    private CraftingStation _craftingStation;
   
    public event Action<CraftingStation> CraftingStationButtonClicked;
    
    public static StationButton Create(CraftingStation craftingStation) {
        StationButton button =
            Manager.Instance.PackedScenes.SelectCraftingStationButton
                .Instantiate<StationButton>();
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
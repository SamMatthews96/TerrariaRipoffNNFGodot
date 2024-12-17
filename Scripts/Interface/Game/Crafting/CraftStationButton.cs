using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class CraftStationButton : TextureButton {
    private CraftingStation _craftingStation;
   
    public event Action<CraftingStation> CraftingStationButtonClicked;
    
    public static CraftStationButton Create(CraftingStation craftingStation) {
        CraftStationButton button =
            Manager.Instance.PackedScenes.SelectCraftingStationButton
                .Instantiate<CraftStationButton>();
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
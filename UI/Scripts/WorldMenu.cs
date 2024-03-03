using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.UI.Scripts;

public partial class WorldMenu : Control {
    [Export] private PackedScene worldButton;
    [Export] private VBoxContainer worldButtonVContainer;

    [Signal]
    public delegate void EnterWorldButtonDownEventHandler();

    [Signal]
    public delegate void BackButtonDownEventHandler();

    [Signal]
    public delegate void CreateWorldButtonDownEventHandler();

    private void OnEnterWorldButtonDown() {
        EmitSignal(SignalName.EnterWorldButtonDown);
    }

    private void OnBackButtonDown() {
        EmitSignal(SignalName.BackButtonDown);
    }

    private void OnCreateWorldButtonDown() {
        EmitSignal(SignalName.CreateWorldButtonDown);
    }

    private void OnWorldAdded(WorldBasicInfo worldBasicInfo) {
        Button newButton = worldButton.Instantiate<Button>();
        newButton.Text = worldBasicInfo.Name;
        worldButtonVContainer.AddChild(newButton);
    }
}
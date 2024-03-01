using Godot;

namespace TerrariaRipoffNNF.UI.Scripts; 

public partial class WorldMenu : Control
{
	[Signal]
	public delegate void EnterWorldButtonDownEventHandler();
	[Signal]
	public delegate void BackButtonDownEventHandler();

	private void OnEnterWorldButtonDown() {
		EmitSignal(SignalName.EnterWorldButtonDown);
	}

	private void OnBackButtonDown() {
		EmitSignal(SignalName.BackButtonDown);
	}
}
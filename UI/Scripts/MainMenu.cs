using Godot;

namespace TerrariaRipoffNNF.UI.Scripts; 

public partial class MainMenu : Control
{
	[Signal]
	public delegate void SinglePlayerButtonDownEventHandler();
	[Signal]
	public delegate void MultiPlayerButtonDownEventHandler();
	[Signal]
	public delegate void QuitButtonDownEventHandler();

	private void OnSinglePlayerButtonDown() {
		EmitSignal(SignalName.SinglePlayerButtonDown);
	}

	private void OnMultiPlayerButtonDown() {
		EmitSignal(SignalName.MultiPlayerButtonDown);
	}

	private void OnExitButtonDown() {
		EmitSignal(SignalName.QuitButtonDown);
	}
}
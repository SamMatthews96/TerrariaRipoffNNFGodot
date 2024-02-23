using Godot;
using System;

public partial class LoginScreen : Node {
	[Signal]
	public delegate void HostButtonDownEventHandler();

	[Signal]
	public delegate void JoinButtonDownEventHandler();

	private void OnHostButtonDown() {
		EmitSignal(SignalName.HostButtonDown);
	}
	private void OnJoinButtonDown() {
		EmitSignal(SignalName.JoinButtonDown);
	}
}

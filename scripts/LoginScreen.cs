using Godot;
using System;
using Microsoft.VisualBasic;

public partial class LoginScreen : Control {
	[Signal]
	public delegate void HostButtonDownEventHandler();

	[Signal]
	public delegate void JoinButtonDownEventHandler();

	private void OnHostButtonDown() {
		Hide();
		EmitSignal(SignalName.HostButtonDown);
	}
	private void OnJoinButtonDown() {
		Hide();
		EmitSignal(SignalName.JoinButtonDown);
	}
}

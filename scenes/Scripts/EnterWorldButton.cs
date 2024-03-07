using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Scenes.Scripts; 

public partial class EnterWorldButton : Button {
	private WorldBasicInfo worldBasicInfo;

	[Signal]
	public delegate void EnterWorldButtonDownEventHandler(WorldBasicInfo worldBasicInfo);
	
	public void Initialize(WorldBasicInfo worldBasicInfo) {
		this.worldBasicInfo = worldBasicInfo; 
		Text = worldBasicInfo.Name;
	}

	private void OnEnterWorldButtonClicked() {
		EmitSignal(SignalName.EnterWorldButtonDown, worldBasicInfo);
	}
}
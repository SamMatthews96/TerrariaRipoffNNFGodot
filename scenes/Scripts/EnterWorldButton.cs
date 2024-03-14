using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Scenes.Scripts; 

public partial class EnterWorldButton : Button {
	private WorldBasicInfo _worldBasicInfo;

	[Signal]
	public delegate void EnterWorldButtonDownEventHandler(WorldBasicInfo worldBasicInfo);
	
	public void Initialize(WorldBasicInfo worldBasicInfo) {
		_worldBasicInfo = worldBasicInfo; 
		Text = worldBasicInfo.Name;
	}

	private void OnEnterWorldButtonDown() {
		EmitSignal(SignalName.EnterWorldButtonDown, _worldBasicInfo);
	}
}
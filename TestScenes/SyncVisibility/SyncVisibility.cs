using Godot;
using TerrariaRipoffNNF;
using TerrariaRipoffNNF.testScenes.SyncVisibility;

/*
 * The results of this test suggest that the only the host visibility is important.
 * The client visibility is not important.
 * 
 * Therefore, when designing the pickup object sync, as it is managed by host, it can
 * be off by default, then once that peer is connected and the sync process is complete,
 * the visibility can be set to true
 */

public partial class SyncVisibility : Node {
    [Export] private NetworkTest _networkTest;
    [Export] private PackedScene _syncObject;
    [Export] private Button _syncVisOn;
    [Export] private Button _syncVisOff;

    private SyncTestObject _syncTestObject;

    public override void _Ready() {
        _networkTest.HostStarted += Start;
        _networkTest.ClientStarted += Start;
    }

    private void Start() {
        _syncTestObject = _syncObject.Instantiate<SyncTestObject>();
        AddChild(_syncTestObject, true);
        _syncVisOn.Pressed += OnSyncVisOnClicked;
        _syncVisOff.Pressed += OnSyncVisOffClicked;
        TreeExiting += () => {
            _syncVisOn.Pressed -= OnSyncVisOnClicked;
            _syncVisOff.Pressed -= OnSyncVisOffClicked;
        };
    }

    private void OnSyncVisOnClicked() {
        int otherPeerId = Multiplayer.GetPeers()[0];
        _syncTestObject.Sync.SetVisibilityFor(otherPeerId, true);
    }

    private void OnSyncVisOffClicked() {
        int otherPeerId = Multiplayer.GetPeers()[0];
        _syncTestObject.Sync.SetVisibilityFor(otherPeerId, false);
    }
}
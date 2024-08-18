using System;
using Godot;
using TerrariaRipoffNNF.Scripts.Resources;

namespace TerrariaRipoffNNF.Scripts.UI;

public partial class EnterWorldButton : Button {
    private WorldBasicInfo _worldBasicInfo;

    public event Action<WorldBasicInfo> EnterWorldButtonDown;

    public void Initialize(WorldBasicInfo worldBasicInfo) {
        _worldBasicInfo = worldBasicInfo;
        Text = worldBasicInfo.Name;
    }

    private void OnEnterWorldButtonDown() {
        EnterWorldButtonDown?.Invoke(_worldBasicInfo);
    }
}
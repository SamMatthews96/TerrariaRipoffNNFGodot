using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class WorldListItem : Control {
    private WorldBasicInfo _worldBasicInfo;
    [Export] private Label _worldNameLabel;
    [Export] private Button _enterWorldButton;
    [Export] private Button _deleteWorldButton;

    public event Action<WorldBasicInfo> SelectWorldButtonDown;

    public void Initialize(WorldBasicInfo worldMetadata) {
        _worldBasicInfo = worldMetadata;
        _worldNameLabel.Text = $"{worldMetadata.Name} - {worldMetadata.Width}x{worldMetadata.Height}";
    }

    public override void _Ready() {
        _enterWorldButton.ButtonDown += OnEnterWorldButtonDown;
        _deleteWorldButton.ButtonDown += OnDeleteWorldButtonDown;
        TreeExiting += () => {
            _enterWorldButton.ButtonDown -= OnEnterWorldButtonDown;
            _deleteWorldButton.ButtonDown -= OnDeleteWorldButtonDown;
        };
    }

    private void OnEnterWorldButtonDown() {
        SelectWorldButtonDown?.Invoke(_worldBasicInfo);
    }

    private void OnDeleteWorldButtonDown() {
        FileManager.DeleteWorld(_worldBasicInfo);
        QueueFree();
    }
}
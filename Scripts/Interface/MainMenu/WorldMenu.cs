using System;
using System.Threading.Tasks;
using Godot;

namespace TerrariaRipoffNNF;

public partial class WorldMenu : Control {
    [Export] private Button _createWorldButton;
    [Export] private Button _backButton;
    [Export] private VBoxContainer _worldListContainer;
    [Export] private PackedScene _packedWorldListItem;
    [Export] private LineEdit _worldNameEdit;
    [Export] private LineEdit _worldWidthEdit;
    [Export] private LineEdit _worldHeightEdit;

    public event Action<WorldBasicInfo> SelectWorldButtonDown;
    public event Action BackButtonDown;

    public override void _Ready() {
        Hide();
        _createWorldButton.ButtonDown += OnCreateWorldButtonDown;
        _backButton.ButtonDown += OnBackButtonDown;

        Task<WorldBasicInfo[]> task = Task.Run(FileManager.LoadAllWorldBasicData);
        task.GetAwaiter().OnCompleted(() => {
            WorldBasicInfo[] worldBasicInfoArray = task.Result;
            foreach (WorldBasicInfo worldBasicInfo in worldBasicInfoArray) {
                AddEnterWorldButton(worldBasicInfo);
            }
        });
    }

    public override void _ExitTree() {
        _createWorldButton.ButtonDown -= OnCreateWorldButtonDown;
        _backButton.ButtonDown -= OnBackButtonDown;
    }

    private void OnSelectWorldButtonDown(WorldBasicInfo worldBasicInfo) {
        Hide();
        SelectWorldButtonDown?.Invoke(worldBasicInfo);
    }

    private void OnBackButtonDown() {
        Hide();
        BackButtonDown?.Invoke();
    }

    private async void OnCreateWorldButtonDown() {
        int width = int.Parse(_worldWidthEdit.Text);
        int height = int.Parse(_worldHeightEdit.Text);
        if (width <= 100) {
            width = 100;
        }

        if (height <= 100) {
            height = 100;
        }
        
        WorldBasicInfo worldBasicInfo = new(_worldNameEdit.Text, width, height);
        AddEnterWorldButton(worldBasicInfo);
        await Task.Run(() => {
            WorldCreator.CreateWorld(worldBasicInfo);
        });
    }

    private void AddEnterWorldButton(WorldBasicInfo worldBasicInfo) {
        WorldListItem worldListItem = _packedWorldListItem.Instantiate<WorldListItem>();
        worldListItem.Initialize(worldBasicInfo);
        worldListItem.SelectWorldButtonDown += OnSelectWorldButtonDown;
        _worldListContainer.AddChild(worldListItem);
    }
}
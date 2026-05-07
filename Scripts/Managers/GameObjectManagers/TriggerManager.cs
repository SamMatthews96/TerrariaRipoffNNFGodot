using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Scripts.Managers.GameObjectManagers;

public partial class TriggerManager : Node {
    private Dictionary<Vector2I, ActiveButton> _triggers = new();

    public void Register(ActiveProp prop, PackedScene scene) {
        ActiveButton button = scene.Instantiate<ActiveButton>();
        prop.AddChild(button);
        button.Triggered += Listener;
        button.TreeExiting += () => {
            button.Triggered -= Listener;
        };
    }

    private void Listener() {
        GD.Print("im listening");
    }
}
using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Scripts.Managers.GameObjectManagers;

public partial class TriggerManager : Node {
    [Export] private World _world;

    private Dictionary<Vector2I, ActiveButton> _triggers = new();
    private Dictionary<Vector2I, ActivePrinter> _actors = new();

    private Dictionary<ActiveButton, Array<ActivePrinter>> 
        _listenersByTrigger = new();
    private Dictionary<ActivePrinter, Array<ActiveButton>> 
        _triggersByListener = new();
    
    public override void _Ready() {
        Action action = () => {
            foreach (ActiveButton button in _triggers.Values) {
                foreach (ActivePrinter printer in _actors.Values) {
                    ConnectSignal(button, printer);
                }
            }
        };
        _world.InputManager.TestPressed += action;
        TreeExiting += () => { _world.InputManager.TestPressed -= action; };
    }

    public void RegisterTrigger(ActiveProp prop, PackedScene scene) {
        ActiveButton button = scene.Instantiate<ActiveButton>();
        button.Scale = prop.Item.GetProperty<ItemProp>().Dimensions;
        prop.AddChild(button);
        _triggers.Add(prop.Anchor, button);
        _listenersByTrigger[button] = new Array<ActivePrinter>();
        button.TreeExiting += () => UnregisterTrigger(prop.Anchor);
    }

    public void RegisterActor(ActiveProp prop, PackedScene scene) {
        ActivePrinter printer = scene.Instantiate<ActivePrinter>();
        prop.AddChild(printer);
        _actors.Add(prop.Anchor, printer);
        _triggersByListener[printer] = new Array<ActiveButton>();
        printer.TreeExiting += () => UnregisterActor(prop.Anchor);
    }

    private void UnregisterTrigger(Vector2I anchor) {
        if (!_triggers.TryGetValue(anchor, out ActiveButton button)) return;

        // Disconnect all event handlers from actors
        if (_listenersByTrigger.TryGetValue(button, out Array<ActivePrinter> actors)) {
            foreach (ActivePrinter actor in actors) {
                button.Triggered -= actor.Action;
                _triggersByListener[actor].Remove(button);
            }
            _listenersByTrigger.Remove(button);
        }

        _triggers.Remove(anchor);
    }

    private void UnregisterActor(Vector2I anchor) {
        if (!_actors.TryGetValue(anchor, out ActivePrinter printer)) return;

        // Disconnect all event handlers from triggers
        if (_triggersByListener.TryGetValue(printer, out Array<ActiveButton> buttons)) {
            foreach (ActiveButton button in buttons) {
                button.Triggered -= printer.Action;
                _listenersByTrigger[button].Remove(printer);
            }
            _triggersByListener.Remove(printer);
        }

        _actors.Remove(anchor);
    }

    private void ConnectSignal(ActiveButton button, ActivePrinter printer) {
        button.Triggered += printer.Action;
        _listenersByTrigger[button].Add(printer);
        _triggersByListener[printer].Add(button);
    }
}
using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class TriggerManager : Node {
    [Export] private World _world;

    private Dictionary<Vector2I, ActiveTrigger> _triggers = new();
    private Dictionary<Vector2I, ActiveActor> _actors = new();

    private Dictionary<ActiveTrigger, Array<ActiveActor>> 
        _listenersByTrigger = new();
    private Dictionary<ActiveActor, Array<ActiveTrigger>> 
        _triggersByListener = new();
    
    public override void _Ready() {
        Action action = () => {
            foreach (ActiveTrigger trigger in _triggers.Values) {
                foreach (ActiveActor actor in _actors.Values) {
                    ConnectSignal(trigger, actor);
                }
            }
        };
        _world.InputManager.TestPressed += action;
        TreeExiting += () => { _world.InputManager.TestPressed -= action; };
    }

    public void RegisterTrigger(ActiveProp prop, PackedScene scene) {
        ActiveTrigger trigger = scene.Instantiate<ActiveTrigger>();
        trigger.Scale = prop.Item.GetProperty<ItemProp>().Dimensions;
        prop.AddChild(trigger);
        _triggers.Add(prop.Anchor, trigger);
        _listenersByTrigger[trigger] = new Array<ActiveActor>();
        trigger.TreeExiting += () => UnregisterTrigger(prop.Anchor);
    }

    public void RegisterActor(ActiveProp prop, PackedScene scene) {
        ActiveActor actor = scene.Instantiate<ActiveActor>();
        prop.AddChild(actor);
        _actors.Add(prop.Anchor, actor);
        _triggersByListener[actor] = new Array<ActiveTrigger>();
        actor.TreeExiting += () => UnregisterActor(prop.Anchor);
    }

    private void UnregisterTrigger(Vector2I anchor) {
        if (!_triggers.TryGetValue(anchor, out ActiveTrigger trigger)) return;

        // Disconnect all event handlers from actors
        if (_listenersByTrigger.TryGetValue(trigger, out Array<ActiveActor> actors)) {
            foreach (ActiveActor actor in actors) {
                trigger.Triggered -= actor.Action;
                _triggersByListener[actor].Remove(trigger);
            }
            _listenersByTrigger.Remove(trigger);
        }

        _triggers.Remove(anchor);
    }

    private void UnregisterActor(Vector2I anchor) {
        if (!_actors.TryGetValue(anchor, out ActiveActor actor)) return;

        // Disconnect all event handlers from triggers
        if (_triggersByListener.TryGetValue(actor, out Array<ActiveTrigger> triggers)) {
            foreach (ActiveTrigger trigger in triggers) {
                trigger.Triggered -= actor.Action;
                _listenersByTrigger[trigger].Remove(actor);
            }
            _triggersByListener.Remove(actor);
        }

        _actors.Remove(anchor);
    }

    private void ConnectSignal(ActiveTrigger trigger, ActiveActor actor) {
        if (_listenersByTrigger[trigger].Contains(actor)) return;

        trigger.Triggered += actor.Action;
        _listenersByTrigger[trigger].Add(actor);
        _triggersByListener[actor].Add(trigger);
    }
}
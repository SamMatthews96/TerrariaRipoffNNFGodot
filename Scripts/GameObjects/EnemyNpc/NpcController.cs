using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;

public partial class NpcController : Node {
    [Export] private EnemyNpc _npc;
    [Export] private Dictionary<string, StateAction> _states;
    [Export] private string _currentStateIndex = "idle";
    [Export] private Timer _stateTimer;
    private StateAction _currentState;
    private Action<EnemyNpc,double> _tick;
    
    public override void _Ready() {
        _currentState = _states[_currentStateIndex];
        _tick = _currentState.Tick;
        _stateTimer.Timeout += StateTimerOnTimeout;
    }

    public override void _PhysicsProcess(double delta) {
        _tick(_npc, delta);
    }

    private void ChangeState(string stateName) {
        _currentState.Exit(_npc);
        _currentState = _states[stateName];
        _tick = _currentState.Tick;
        _currentState.Enter(_npc);
    }

    private void StateTimerOnTimeout() {
        string transitionName = _currentState.GetTransition(_npc);
        if (transitionName is null) return;
        ChangeState(transitionName);
    }
}
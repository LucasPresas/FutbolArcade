using Godot;
using System.Collections.Generic;
using System.Diagnostics;

public partial class PlayerStateMachine : Node
{
    [Export] public State InitialState;
    [Export] public float MinChangeInterval = 0.15f; // segundos mínimos entre cambios

    private Dictionary<string, State> _states = new Dictionary<string, State>();
    private State _currentState;
    private double _lastChangeTime = -9999;

    // Stopwatch compartido para medir tiempo con precisión sin depender de OS.*
    private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    // Propiedad pública de solo lectura para que otros componentes consulten el estado actual
    public string CurrentStateName
    {
        get { return _currentState != null ? _currentState.Name.ToString().ToLower() : string.Empty; }
    }

    // Helper de tiempo usando Stopwatch (compatible con .NET y Godot 4.6)
    private double NowSeconds() => _stopwatch.Elapsed.TotalSeconds;

    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is State state)
            {
                _states[child.Name.ToString().ToLower()] = state;
            }
        }

        if (InitialState != null)
        {
            ChangeState(InitialState.Name);
        }
    }

    public void ChangeState(string newStateName)
    {
        if (string.IsNullOrEmpty(newStateName)) return;

        string name = newStateName.ToLower();
        if (!_states.ContainsKey(name)) return;

        // Cooldown: evita cambios demasiado rápidos
        double now = NowSeconds();
        if (now - _lastChangeTime < MinChangeInterval) return;

        // Evita reentradas al mismo estado
        if (_currentState != null && _currentState == _states[name]) return;

        _currentState?.Exit();
        _currentState = _states[name];
        _currentState.Enter();

        _lastChangeTime = now;
        GD.Print($"[PlayerVM] Estado cambiado a: {name}");
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsUpdate((float)delta);
    }
}

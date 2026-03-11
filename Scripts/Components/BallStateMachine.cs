using Godot;
using System.Collections.Generic;

public partial class BallStateMachine : Node
{
    // Cambiamos 'BallState' por 'State' para que coincida con la herencia
    [Export] public State InitialState; 

    private Dictionary<string, State> _states = new Dictionary<string, State>();
    private State _currentState;

    public override void _Ready()
    {
        Ball ball = GetOwner<Ball>();
        
        foreach (Node child in GetChildren())
        {
            if (child is State state)
            {
                _states[child.Name.ToString().ToLower()] = state;
            }
        }

        if (InitialState != null)
            ChangeState(InitialState.Name);
    }

    public void ChangeState(string newStateName)
    {
        string name = newStateName.ToLower();
        if (!_states.ContainsKey(name)) return;

        _currentState?.Exit();
        _currentState = _states[name];
        _currentState.Enter();
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsUpdate((float)delta);
    }
}
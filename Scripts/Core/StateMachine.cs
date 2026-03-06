using Godot;

public partial class StateMachine : Node
{
    [Export] 
    public NodePath InitialState { get; set; }

    private State _currentState;

    public override void _Ready()
    {
        if (InitialState != null)
        {
            _currentState = GetNode<State>(InitialState);
            _currentState.Enter();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_currentState != null)
        {
            _currentState.Update(delta);
        }
    }

    public void TransitionTo(string stateName, Godot.Collections.Dictionary<string, Variant> message = null)
    {
        if (!HasNode(stateName))
        {
            GD.PrintErr($"State {stateName} not found in StateMachine {Name}");
            return;
        }

        var nextState = GetNode<State>(stateName);

        if (_currentState != null)
        {
            _currentState.Exit();
        }

        _currentState = nextState;
        _currentState.Enter(message);
    }
}

using Godot;

public abstract partial class State : Node
{
    protected StateMachine StateMachine;

    public override void _Ready()
    {
        StateMachine = GetParent<StateMachine>();
    }

    public virtual void Enter(Godot.Collections.Dictionary<string, Variant> message = null)
    {
        // Setup code here
    }

    public virtual void Exit()
    {
        // Cleanup code here
    }

    public virtual void Update(double delta)
    {
        // Custom logic here, override in specific states
    }
}

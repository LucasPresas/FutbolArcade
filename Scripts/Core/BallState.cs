using Godot;

public abstract partial class BallState : Node
{
    protected Ball BallNode;
    protected BallStateMachine Machine;

    public void Init(Ball ball, BallStateMachine machine)
    {
        BallNode = ball;
        Machine = machine;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(float delta) { }
}
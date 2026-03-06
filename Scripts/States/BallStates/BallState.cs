using Godot;

public abstract partial class BallState : State
{
    protected Ball Ball => GetParent().GetParent<Ball>();
}

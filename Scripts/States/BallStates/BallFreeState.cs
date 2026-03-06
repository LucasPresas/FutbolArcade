using Godot;

public partial class BallFreeState : BallState
{
    public override void Enter(Godot.Collections.Dictionary<string, Variant> message = null)
    {
        // Reactivate collisions when the ball is free
        Ball.SetDeferred(RigidBody2D.PropertyName.CollisionLayer, 1);
        Ball.SetDeferred(RigidBody2D.PropertyName.CollisionMask, 1);
    }

    public override void Update(double delta)
    {
        // Apply custom friction or drag if Godot's built-in damping isn't quite right for a football
    }
}

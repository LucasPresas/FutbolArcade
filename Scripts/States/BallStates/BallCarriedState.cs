using Godot;

public partial class BallCarriedState : BallState
{
    private Node3D _carrierMarker;

    public override void Enter(Godot.Collections.Dictionary<string, Variant> message = null)
    {
        // Disable physical collisions so the ball passes through things while carried
        Ball.SetDeferred(RigidBody3D.PropertyName.CollisionLayer, 0);
        Ball.SetDeferred(RigidBody3D.PropertyName.CollisionMask, 0);
        
        // Kill momentum
        Ball.LinearVelocity = Vector3.Zero;
        Ball.AngularVelocity = Vector3.Zero;

        if (message != null && message.ContainsKey("carrier_marker"))
        {
            _carrierMarker = message["carrier_marker"].As<Node3D>();
        }
    }

    public override void Update(double delta)
    {
        // Soft snap to the player's dribble point using GlobalPosition
        if (_carrierMarker != null && IsInstanceValid(_carrierMarker))
        {
            Ball.GlobalPosition = _carrierMarker.GlobalPosition;
        }
    }

    public override void Exit()
    {
        _carrierMarker = null;
    }
}

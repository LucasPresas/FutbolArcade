using Godot;

public partial class PitchBoundary : StaticBody2D
{
    [Export] public float BounceForce { get; set; } = 0.8f; // 0 is no bounce, 1 is perfect bounce (billiards)

    public override void _Ready()
    {
        // Enforce the bounce material at runtime just in case it wasn't set in the editor
        if (PhysicsMaterialOverride == null)
        {
            PhysicsMaterialOverride = new PhysicsMaterial();
        }
        
        PhysicsMaterialOverride.Bounce = BounceForce;
        
        // Ensure friction is relatively low so the ball doesn't stop immediately when hitting a wall
        PhysicsMaterialOverride.Friction = 0.1f; 
    }
}

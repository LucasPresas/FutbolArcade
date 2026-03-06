using Godot;

public partial class MovementComponent : ComponentBase
{
    [Export] public float MaxSpeed { get; set; } = 8.0f;
    [Export] public float Acceleration { get; set; } = 40.0f;
    [Export] public float Friction { get; set; } = 40.0f;

    /// <summary>
    /// Call this inside _PhysicsProcess of a State to apply movement.
    /// </summary>
    public void Move(Vector3 inputDirection, double delta)
    {
        if (Actor == null) return;

        Vector3 velocity = Actor.Velocity;

        if (inputDirection != Vector3.Zero)
        {
            // Accelerate towards the input direction
            velocity = velocity.MoveToward(inputDirection * MaxSpeed, Acceleration * (float)delta);
            
            // Optional: Flip visually if we have a Visuals node
            // Flipping logic should ideally be in another component or ActorBase, but keeping it simple here.
        }
        else
        {
            // Apply friction
            velocity = velocity.MoveToward(Vector3.Zero, Friction * (float)delta);
        }

        // Preserve original Y velocity for gravity from CharacterBase
        velocity.Y = Actor.Velocity.Y;
        
        // Asignamos la velocidad calculada al CharacterBody3D
        Actor.Velocity = velocity;
        
        // MoveAndSlide integra la velocidad
        Actor.MoveAndSlide();

        // Rotate the entire Actor so GrabArea + DribblePoint face movement direction
        // Negate both components: Atan2(-X, -Z) makes -Z local face the input direction
        if (inputDirection.LengthSquared() > 0)
        {
            float targetAngle = Mathf.Atan2(-inputDirection.X, -inputDirection.Z);
            Actor.Rotation = new Vector3(0, targetAngle, 0);
        }
    }
}

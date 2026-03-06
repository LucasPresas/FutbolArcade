using Godot;

public partial class MovementComponent : ComponentBase
{
    [Export] public float MaxSpeed { get; set; } = 300.0f;
    [Export] public float Acceleration { get; set; } = 1500.0f;
    [Export] public float Friction { get; set; } = 1200.0f;

    /// <summary>
    /// Call this inside _PhysicsProcess of a State to apply movement.
    /// </summary>
    public void Move(Vector2 inputDirection, double delta)
    {
        if (Actor == null) return;

        Vector2 velocity = Actor.Velocity;

        if (inputDirection != Vector2.Zero)
        {
            // Accelerate towards the input direction
            velocity = velocity.MoveToward(inputDirection * MaxSpeed, Acceleration * (float)delta);
            
            // Optional: Flip visually if we have a Visuals node
            // Flipping logic should ideally be in another component or ActorBase, but keeping it simple here.
        }
        else
        {
            // Apply friction
            velocity = velocity.MoveToward(Vector2.Zero, Friction * (float)delta);
        }

        Actor.Velocity = velocity;
        Actor.MoveAndSlide();
    }
}

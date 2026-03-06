using Godot;

public partial class PlayerInputController : Node, IController
{
    [Export] public string PlayerId { get; set; } = "1"; // "1", "2", "3", "4" for local multiplayer

    public Vector2 GetMoveDirection()
    {
        Vector2 direction = Vector2.Zero;
        
        // Example: Joypad support or Keyboard support based on PlayerId
        // This can be expanded later to use Godot's Input Actions properly.
        if (Input.IsActionPressed($"p{PlayerId}_up")) direction.Y -= 1;
        if (Input.IsActionPressed($"p{PlayerId}_down")) direction.Y += 1;
        if (Input.IsActionPressed($"p{PlayerId}_left")) direction.X -= 1;
        if (Input.IsActionPressed($"p{PlayerId}_right")) direction.X += 1;

        return direction.Normalized();
    }

    public bool IsPassing() => Input.IsActionJustPressed($"p{PlayerId}_pass");
    
    public bool IsShooting() => Input.IsActionJustPressed($"p{PlayerId}_shoot");
    
    public bool IsTackling() => Input.IsActionJustPressed($"p{PlayerId}_tackle");
}

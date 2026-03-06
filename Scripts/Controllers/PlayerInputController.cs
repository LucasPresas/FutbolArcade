using Godot;

public partial class PlayerInputController : Node, IController
{
    [Export] public string PlayerId { get; set; } = "1"; // "1", "2", "3", "4" for local multiplayer

    public Vector3 GetMoveDirection()
    {
        Vector3 direction = Vector3.Zero;
        
        // Example: Joypad support or Keyboard support based on PlayerId.
        // For Player 1 (default mappings in the original repo):
        string suffix = PlayerId == "1" ? "" : $"_p{PlayerId}";
        
        if (Input.IsActionPressed($"move_up{suffix}")) direction.Z -= 1;
        if (Input.IsActionPressed($"move_down{suffix}")) direction.Z += 1;
        if (Input.IsActionPressed($"move_left{suffix}")) direction.X -= 1;
        if (Input.IsActionPressed($"move_right{suffix}")) direction.X += 1;

        return direction.Normalized();
    }

    // Default to the original repo's action_pass and action_shoot
    public bool IsPassing() => Input.IsActionJustPressed(PlayerId == "1" ? "action_pass" : $"action_pass_p{PlayerId}");
    public bool IsShooting() => Input.IsActionJustPressed(PlayerId == "1" ? "action_shoot" : $"action_shoot_p{PlayerId}");
    public bool IsTackling() => Input.IsActionJustPressed(PlayerId == "1" ? "action_tackle" : $"action_tackle_p{PlayerId}");
}

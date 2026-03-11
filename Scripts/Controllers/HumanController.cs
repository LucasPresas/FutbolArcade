using Godot;

public partial class HumanController : Node, IController
{
    public Vector3 GetMoveDirection()
    {
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        // Convertimos a 3D (Plano XZ)
        return new Vector3(inputDir.X, 0, inputDir.Y).Normalized();
    }

    public bool IsShooting() => Input.IsActionJustPressed("action_shoot");
    public bool IsPassing() => Input.IsActionJustPressed("action_pass");
    public bool IsTackling() => Input.IsActionJustPressed("action_tackle");
    public bool IsCharging() => Input.IsActionPressed("action_shoot");
}
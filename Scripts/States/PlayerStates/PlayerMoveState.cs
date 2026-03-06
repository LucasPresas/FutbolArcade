using Godot;

public partial class PlayerMoveState : PlayerState
{
    public override void Enter(Godot.Collections.Dictionary<string, Variant> message = null)
    {
        // Example: Play "run" animation here
    }

    public override void Update(double delta)
    {
        Vector2 inputDir = Player.Controller?.GetMoveDirection() ?? Vector2.Zero;

        // Transition to idle state if input stops
        if (inputDir == Vector2.Zero)
        {
            StateMachine.TransitionTo("Idle");
            return;
        }

        // Apply movement vector
        Movement?.Move(inputDir, delta);
    }
}

using Godot;

public partial class PlayerMoveState : PlayerState
{
    public override void Enter(Godot.Collections.Dictionary<string, Variant> message = null)
    {
        // Example: Play "run" animation here
    }

    public override void Update(double delta)
    {
        Vector3 inputDir = Player.Controller?.GetMoveDirection() ?? Vector3.Zero;

        // Transition to idle state if input stops
        if (inputDir == Vector3.Zero)
        {
            StateMachine.TransitionTo("Idle");
            return;
        }

        // Apply movement vector
        Movement?.Move(inputDir, delta);
    }
}

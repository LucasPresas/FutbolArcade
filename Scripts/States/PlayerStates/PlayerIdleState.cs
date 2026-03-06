using Godot;

public partial class PlayerIdleState : PlayerState
{
    public override void Enter(Godot.Collections.Dictionary<string, Variant> message = null)
    {
        // Example: Play "idle" animation here
    }

    public override void Update(double delta)
    {
        Vector2 inputDir = Player.Controller?.GetMoveDirection() ?? Vector2.Zero;
        
        // Transition to move state if there's input
        if (inputDir != Vector2.Zero)
        {
            StateMachine.TransitionTo("Move");
            return;
        }

        // Apply friction to completely stop if still sliding a bit
        Movement?.Move(Vector2.Zero, delta);
    }
}

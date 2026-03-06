using Godot;

public partial class GoalArea : Area3D
{
    [Export] public int TeamId { get; set; } // 0 or 1, representing which team's score should increase

    public override void _Ready()
    {
        AddToGroup($"goal_{TeamId}");
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is Ball ball)
        {
            // Emit the signal directly through the EventBus
            EventBus.Instance.EmitSignal(EventBus.SignalName.GoalScored, TeamId);
            
            // Optional: reset ball velocity or disable its physics immediately
            ball.LinearVelocity = Vector3.Zero;
        }
    }
}

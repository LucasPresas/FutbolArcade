using Godot;

public partial class BallHandlerComponent : ComponentBase
{
    [Export] public NodePath DetectionAreaPath { get; set; }
    [Export] public NodePath DribbleMarkerPath { get; set; }

    private Area3D _detectionArea;
    private Marker3D _dribbleMarker;

    // Publicly accessible so Player.TryAction can kick the carried ball
    public Ball CarriedBall { get; private set; }

    public override void _Ready()
    {
        base._Ready(); // Link to ActorBase

        if (DetectionAreaPath != null)
        {
            _detectionArea = GetNode<Area3D>(DetectionAreaPath);
            _detectionArea.BodyEntered += OnBodyEntered;
        }

        if (DribbleMarkerPath != null)
        {
            _dribbleMarker = GetNode<Marker3D>(DribbleMarkerPath);
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        // Only pick up if we don't already have the ball
        if (body is Ball ball && CarriedBall == null)
        {
            CarriedBall = ball;

            var message = new Godot.Collections.Dictionary<string, Variant>
            {
                { "carrier_marker", _dribbleMarker }
            };

            ball.StateMachine.TransitionTo("Carried", message);
            GD.Print($"[BallHandler] {Actor.Name} agarró la pelota.");
        }
    }

    public void ReleaseBall()
    {
        CarriedBall = null;
    }
}

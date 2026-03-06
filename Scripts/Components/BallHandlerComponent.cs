using Godot;

public partial class BallHandlerComponent : ComponentBase
{
    [Export] public NodePath DetectionAreaPath { get; set; }
    [Export] public NodePath DribbleMarkerPath { get; set; }

    private Area2D _detectionArea;
    private Marker2D _dribbleMarker;

    public override void _Ready()
    {
        base._Ready(); // Link to ActorBase

        if (DetectionAreaPath != null)
        {
            _detectionArea = GetNode<Area2D>(DetectionAreaPath);
            // Connect signal to detect ball
            _detectionArea.BodyEntered += OnBodyEntered;
        }

        if (DribbleMarkerPath != null)
        {
            _dribbleMarker = GetNode<Marker2D>(DribbleMarkerPath);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        // When the Area2D touches the ball
        if (body is Ball ball)
        {
            // Transition ball to carried state, passing the marker for the ball to snap to
            var message = new Godot.Collections.Dictionary<string, Variant>
            {
                { "carrier_marker", _dribbleMarker }
            };
            
            ball.StateMachine.TransitionTo("Carried", message);
            
            // Optionally, transition the player to Dribble mode if that state exists
            if (Actor.StateMachine.HasNode("Dribble"))
            {
                Actor.StateMachine.TransitionTo("Dribble");
            }
        }
    }
}

using Godot;

public partial class MatchManager : Node
{
    private int _team0Score = 0;
    private int _team1Score = 0;

    private Label _scoreLabel;
    private Ball _ball;
    private Vector3 _ballStartPos;

    public override void _Ready()
    {
        EventBus.Instance.GoalScored += OnGoalScored;

        var root = GetParent();
        _scoreLabel = root.GetNodeOrNull<Label>("UI/ScoreLabel");
        _ball = root.GetNodeOrNull<Ball>("Ball");
        if (_ball != null) _ballStartPos = _ball.GlobalPosition;

        // Force a clean start on the next frame to assign the ball
        CallDeferred(nameof(ResetPositions));
    }

    private void OnGoalScored(int teamId)
    {
        if (teamId == 0)      { _team0Score++; GD.Print($"GOL Team 0! {_team0Score}-{_team1Score}"); }
        else if (teamId == 1) { _team1Score++; GD.Print($"GOL Team 1! {_team0Score}-{_team1Score}"); }

        if (_scoreLabel != null)
            _scoreLabel.Text = $"{_team0Score}  -  {_team1Score}";

        CallDeferred(nameof(ResetPositions));
    }

    private void ResetPositions()
    {
        ActorBase startingPlayer = null;

        foreach (var node in GetTree().GetNodesInGroup("all_players"))
        {
            if (node is ActorBase actor)
            {
                actor.ResetToStart();
                
                // For this prototype, the player closest to the center starts with the ball
                // (Our main player is at X = -1)
                if (startingPlayer == null || actor.GlobalPosition.DistanceSquaredTo(Vector3.Zero) < startingPlayer.GlobalPosition.DistanceSquaredTo(Vector3.Zero))
                {
                    startingPlayer = actor;
                }
            }
        }

        if (_ball != null)
        {
            _ball.GlobalPosition = _ballStartPos;
            _ball.LinearVelocity = Vector3.Zero;
            _ball.AngularVelocity = Vector3.Zero;
            _ball.StateMachine?.TransitionTo("Free");

            if (startingPlayer != null)
            {
                // Force possession to the starting player
                var ballHandler = startingPlayer.GetNodeOrNull<BallHandlerComponent>("Components/BallHandler");
                if (ballHandler != null)
                {
                    // Small hack to ensure safe attachment without physics collision
                    _ball.GlobalPosition = startingPlayer.GlobalPosition;
                }
            }
        }
    }

    public override void _ExitTree()
    {
        if (EventBus.Instance != null)
            EventBus.Instance.GoalScored -= OnGoalScored;
    }
}

using Godot;
using System;

public partial class MatchManager : Node
{
    [Export] public float ResetDelay = 3.0f;
    private Ball _ball;

    public override void _Ready()
    {
        _ball = GetTree().CurrentScene.FindChild("Ball") as Ball;
        Goal.OnGoalScored += HandleGoal;
    }

    public override void _ExitTree()
    {
        Goal.OnGoalScored -= HandleGoal;
    }

    private void HandleGoal(string teamName)
    {
        if (!IsInstanceValid(this) || !IsInsideTree()) return;
        SetPlayersActive(false);
        GetTree().CreateTimer(ResetDelay).Timeout += ResetMatch;
    }

    private void ResetMatch()
    {
        if (!IsInstanceValid(this) || !IsInsideTree()) return;

        if (_ball != null)
        {
            _ball.GlobalPosition = new Vector3(0, 2, 0);
            _ball.LinearVelocity = Vector3.Zero;
            _ball.AngularVelocity = Vector3.Zero;
        }

        foreach (Node node in GetTree().GetNodesInGroup("Players"))
        {
            if (node is PlayerBase player) player.ResetToInitialPosition();
        }

        SetPlayersActive(true);
    }

    private void SetPlayersActive(bool active)
    {
        if (!IsInstanceValid(this) || !IsInsideTree()) return;
        foreach (Node node in GetTree().GetNodesInGroup("Players"))
        {
            node.SetProcess(active);
            node.SetPhysicsProcess(active);
        }
    }
}
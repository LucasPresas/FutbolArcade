using Godot;

public partial class MatchManager : Node
{
    private int _team1Score = 0;
    private int _team2Score = 0;

    public override void _Ready()
    {
        // Subscribe to goal events
        EventBus.Instance.GoalScored += OnGoalScored;
    }

    private void OnGoalScored(int teamId)
    {
        if (teamId == 0)
        {
            _team1Score++;
            GD.Print($"GOAL for Team 1! Score: {_team1Score} - {_team2Score}");
        }
        else if (teamId == 1)
        {
            _team2Score++;
            GD.Print($"GOAL for Team 2! Score: {_team1Score} - {_team2Score}");
        }

        // Logic to reset positions, wait for kickoff, etc.
    }

    public override void _ExitTree()
    {
        // Clean up events
        if (EventBus.Instance != null)
        {
            EventBus.Instance.GoalScored -= OnGoalScored;
        }
    }
}

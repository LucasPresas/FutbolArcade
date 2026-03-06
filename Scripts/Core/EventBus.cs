using Godot;

// Singleton to handle global events
public partial class EventBus : Node
{
    // C# Events
    
    // UI Events
    [Signal]
    public delegate void GoalScoredEventHandler(int teamId); // teamId: 0 for Team A, 1 for Team B
    
    [Signal]
    public delegate void MatchStartedEventHandler();
    
    [Signal]
    public delegate void MatchEndedEventHandler();

    // In a real project, we would use Godot's Autoload feature to make this globally accessible.
    // E.g., EventBus.Instance.EmitSignal(EventBus.SignalName.GoalScored, 0);

    public static EventBus Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree(); // Make sure only one EventBus exists
        }
    }
}

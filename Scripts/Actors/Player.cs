using Godot;

public partial class Player : ActorBase
{
    // This is the concrete player implementation.
    // The actual physics and state transitions are handled by ComponentBase and State classes,
    // following the architectural guidelines.
    
    public override void _Ready()
    {
        base._Ready(); // Call ActorBase _Ready to setup StateMachine & Controller references
        
        // Example: Auto-assign player controller if one exists in the expected hierarchy
        if (Controller == null && HasNode("Controllers/PlayerInput"))
        {
            SetController(GetNode<IController>("Controllers/PlayerInput"));
        }
    }
}

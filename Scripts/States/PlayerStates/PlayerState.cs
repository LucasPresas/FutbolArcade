using Godot;

public abstract partial class PlayerState : State
{
    // Helper property to access the player specific actor base
    protected ActorBase Player => GetParent().GetParent<ActorBase>();
    
    // Helper property to access the movement component
    protected MovementComponent Movement => Player.GetNodeOrNull<MovementComponent>("Components/Movement");
}

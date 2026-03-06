using Godot;

public abstract partial class PlayerState : State
{
    // Helper property to access the player specific actor base
    protected Player Player => GetParent().GetParent<Player>();
    
    // Helper property to access the movement component
    protected MovementComponent Movement => Player.GetNodeOrNull<MovementComponent>("Components/Movement");
}

using Godot;

public partial class Ball : RigidBody2D // Or RigidBody3D
{
    // The ball will also have a StateMachine to handle Freedom, Carried, Passed, Shot
    [Export] public NodePath StateMachinePath { get; set; }

    public StateMachine StateMachine { get; private set; }

    public override void _Ready()
    {
        if (StateMachinePath != null)
        {
            StateMachine = GetNode<StateMachine>(StateMachinePath);
        }
    }
}

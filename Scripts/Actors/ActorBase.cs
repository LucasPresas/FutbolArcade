using Godot;

public partial class ActorBase : CharacterBody2D // Or CharacterBody3D if you chose Option B for 2DHD
{
    [Export] public NodePath StateMachinePath { get; set; }
    [Export] public NodePath ControllerPath { get; set; }

    public StateMachine StateMachine { get; private set; }
    public IController Controller { get; set; }

    public override void _Ready()
    {
        if (StateMachinePath != null)
        {
            StateMachine = GetNode<StateMachine>(StateMachinePath);
        }

        if (ControllerPath != null)
        {
            Controller = GetNode<IController>(ControllerPath);
        }
    }

    public void SetController(IController newController)
    {
        Controller = newController;
    }
}

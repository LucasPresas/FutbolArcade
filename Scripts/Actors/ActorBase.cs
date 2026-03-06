using Godot;
using AcmeStriker;

public partial class ActorBase : CharacterBase // Bridging to the user's original placeholder base
{
	[Export] public NodePath StateMachinePath { get; set; }
	[Export] public NodePath ControllerPath { get; set; }
	[Export] public int TeamId { get; set; } = 0;

	public StateMachine StateMachine { get; private set; }
	public IController Controller { get; set; }

	private Vector3 _startPosition;

	public override void _Ready()
	{
		base._Ready(); // Call original Ready to setup Visuals

		_startPosition = GlobalPosition;
		AddToGroup("all_players");

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

	public void ResetToStart()
	{
		GlobalPosition = _startPosition;
		Velocity = Vector3.Zero;
	}
}

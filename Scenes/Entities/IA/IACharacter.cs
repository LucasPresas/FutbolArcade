using Godot;
using System;

namespace AcmeStriker;

public partial class IACharacter : CharacterBase
{
	[ExportGroup("IA BrIAn Settings")]
	[Export] public float RandomWanderRadius = 4.0f; // Qué tan lejos merodea de la pelota
	[Export] public float SupportDistance = 5.0f;    // Qué tan lejos sigue a un compañero

	[ExportGroup("IA Components")]
	[Export] public NavigationAgent3D NavAgent; 

	private Node3D _ball;
	
	// Nuestro FSM (Máquina de Estados)
	private enum IABehavior { Chasing, Wandering, Supporting }
	private IABehavior _currentBehavior = IABehavior.Wandering;

	public override void _Ready()
	{

		if (NavAgent == null)
		{
			GD.PushError("¡Cuidado! Asigna el NavigationAgent3D en el Inspector del IAPlayer.");
		}

		// Buscamos la pelota usando Grupos (Desacoplamiento total)
		_ball = GetTree().GetFirstNodeInGroup("Ball") as Node3D;
	
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_ball == null || NavAgent == null)
		{
			Console.WriteLine("no se encuentra la pelota");
		} else
		{
			Console.WriteLine("se encuentra la pelota");
		}

		DetermineBehavior();
		ExecuteMovement();
	}

	// Aquí está el método que el compilador no encontraba
	private void DetermineBehavior()
	{
		// TODO: En un futuro, el "BallManager" nos dirá quién tiene la pelota.
		// Por ahora, simulamos las condiciones que pidió el Game Designer:
		bool ballIsFree = true; 
		bool teammateHasBall = false;
	
		if (teammateHasBall)
		{
			// Si mi compañero la tiene, me muevo a cierta distancia (Supporting)
			_currentBehavior = IABehavior.Supporting;
		}
		else if (ballIsFree) 
		{
			// Si nadie la tiene, tiro un dado: 70% de perseguir directo, 30% de merodear
			// Esto le da el toque "azar" alrededor de la pelota que pediste.
			_currentBehavior = (GD.Randf() > 0.3f) ? IABehavior.Chasing : IABehavior.Wandering;
		}
	}

	// Aquí está el método para movernos según lo que decidimos arriba
	private void ExecuteMovement()
	{
		Vector3 targetPos = _ball.GlobalPosition;

		switch (_currentBehavior)
		{
			case IABehavior.Chasing:
				// Va directo a la pelota, no modificamos targetPos
				break;

			case IABehavior.Wandering:
				// Merodeo azaroso alrededor del punto donde está la pelota
				Vector3 randomOffset = new Vector3((float)GD.RandRange(-1, 1), 0, (float)GD.RandRange(-1, 1)).Normalized();
				targetPos += randomOffset * RandomWanderRadius;
				break;

			case IABehavior.Supporting:
				// Actúa moviéndose a cierta distancia de la pelota, siguiéndola
				Vector3 awayFromBall = (GlobalPosition - _ball.GlobalPosition).Normalized();
				targetPos = _ball.GlobalPosition + (awayFromBall * SupportDistance);
				break;
		}

		// Le decimos al cerebro de navegación adónde queremos ir
		NavAgent.TargetPosition = targetPos;

		// Si aún no llegamos al destino, nos movemos
		if (!NavAgent.IsTargetReached())
		{
			Vector3 nextPoint = NavAgent.GetNextPathPosition();
			Vector3 direction = (nextPoint - GlobalPosition).Normalized();
			
			// Usamos la función de la clase Base (CharacterBase.cs) para la física real
			MoveCharacter(direction);
		}
	}
}

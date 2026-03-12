using Godot;
using System;

[GlobalClass] 
public partial class Goal : Area3D
{
    [Export] public string GoalType = "Local"; 

    // 1. EVENTO GLOBAL (Mini EventBus): Avisa a todo el juego que hubo gol
    public static event Action<string> OnGoalScored; 

    private CollisionShape3D _collisionShape;

    public override void _Ready()
    {
        BodyEntered += OnBallEntered;
        // Asumiendo que tu nodo de colisión hijo se llama "CollisionShape3D"
        _collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
    }

    private async void OnBallEntered(Node3D body)
    {
        if (body is Ball ball)
        {
            // 2. Apagamos el arco temporalmente para evitar que detecte el gol 60 veces por segundo
            _collisionShape.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);

            string scorer = (GoalType == "Local") ? "Visitante" : "Local";
            
            GD.Print("----------------------------------");
            GD.Print($"¡¡¡ GOL DEL EQUIPO {scorer.ToUpper()} !!!");
            GD.Print("----------------------------------");

            // 3. Avisamos al aire que hubo gol (Los jugadores escucharán esto para soltar la pelota)
            OnGoalScored?.Invoke(scorer);

            // 4. Forzamos a la pelota a quedar "Libre" para que deje de seguir al jugador
            var ballMachine = ball.GetNodeOrNull<BallStateMachine>("StateMachine");
            if (ballMachine != null) ballMachine.ChangeState("Free");

            // 5. Frenamos la pelota adentro de la red
            ball.LinearVelocity = Vector3.Zero;
            ball.AngularVelocity = Vector3.Zero;

            // 6. DELAY DE 0.5 SEGUNDOS (Usando la forma nativa de Godot C#)
            await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);

            // 7. Reseteamos la posición y físicas al centro de la cancha
            ball.GlobalPosition = Vector3.Zero;
            ball.LinearVelocity = Vector3.Zero;
            ball.AngularVelocity = Vector3.Zero;

            // 8. Volvemos a encender el arco
            _collisionShape.SetDeferred(CollisionShape3D.PropertyName.Disabled, false);
        }
    }
}
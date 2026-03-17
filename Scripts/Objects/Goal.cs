using Godot;
using System;

[GlobalClass] 
public partial class Goal : Area3D
{
    [Export] public string GoalType = "Local"; 
    [Export] public float GoalCooldown = 3.0f; // Tiempo de seguridad

    public static event Action<string> OnGoalScored; 

    private CollisionShape3D _collisionShape;
    private bool _isActive = true;

    public override void _Ready()
    {
        BodyEntered += OnBallEntered;
        _collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
    }

    private void OnBallEntered(Node3D body)
    {
        // SEGURIDAD: Solo procesar si el arco está activo y es la pelota
        if (!_isActive || !(body is Ball ball)) return;

        _isActive = false;
        // Desactivamos la colisión físicamente para evitar dobles detecciones
        _collisionShape.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);

        string scorer = (GoalType == "Local") ? "Visitante" : "Local";
        
        GD.Print($"¡¡¡ GOL DEL EQUIPO {scorer.ToUpper()} !!!");

        // 1. Avisamos al MatchManager y a los jugadores
        OnGoalScored?.Invoke(scorer);

        // 2. Liberamos la pelota de cualquier jugador que la lleve
        var ballMachine = ball.GetNodeOrNull<BallStateMachine>("StateMachine");
        if (ballMachine != null) ballMachine.ChangeState("Free");

        // 3. Frenamos la pelota (El MatchManager se encargará de posicionarla luego)
        ball.LinearVelocity = Vector3.Zero;
        ball.AngularVelocity = Vector3.Zero;

        // 4. COOLDOWN: No reactivamos el arco inmediatamente. 
        // Esperamos a que el MatchManager haga el reset de todo.
        GetTree().CreateTimer(GoalCooldown).Timeout += ResetGoal;
    }

    private void ResetGoal()
    {
        _isActive = true;
        _collisionShape.SetDeferred(CollisionShape3D.PropertyName.Disabled, false);
        GD.Print($"Arco {GoalType} reactivado.");
    }
}
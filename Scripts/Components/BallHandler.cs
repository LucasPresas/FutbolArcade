using Godot;
using System;

public partial class BallHandler : Node
{
    private PlayerBase _player;
    private Area3D _grabArea;
    private Marker3D _dribblePoint;
    
    private Ball _currentBall;
    private bool _hasBall = false;

    // ==========================================
    // VARIABLES EXPUESTAS AL INSPECTOR
    // ==========================================
    [ExportGroup("Físicas de Tiro (Remate)")]
    [Export] public float MinKickElevation = 0.05f; // Toque rápido (casi rasante)
    [Export] public float MaxKickElevation = 0.4f;  // Tiro cargado al máximo (parábola)
    [Export] public float MinKickForceMult = 0.6f;  // 60% de la fuerza base
    [Export] public float MaxKickForceMult = 1.3f;  // 130% de la fuerza base (tiro potente)
    [Export] public float KickFriction = 0.5f;      // Que ruede mucho

    [ExportGroup("Físicas de Pase")]
    [Export] public float PassElevation = 0.02f;    // Apenitas levantado para evitar fricción
    [Export] public float PassForceMult = 0.6f;
    [Export] public float PassFriction = 2.5f;      // Que frene al llegar al compañero

    public override void _Ready()
    {
        _player = GetOwner<PlayerBase>();
        _grabArea = _player.GetNode<Area3D>("Rotator/GrabArea");
        _dribblePoint = _player.GetNode<Marker3D>("Rotator/DribblePoint");

        _grabArea.BodyEntered += OnBallEntered;
    }

    public override void _EnterTree()
    {
        Goal.OnGoalScored += HandleGoalScored;
    }

    public override void _ExitTree()
    {
        Goal.OnGoalScored -= HandleGoalScored;
    }

    private void HandleGoalScored(string scorerTeam)
    {
        if (_hasBall)
        {
            _hasBall = false;
            _currentBall = null;
            _player.StateMachine.ChangeState("Idle"); 
        }
    }

    private void OnBallEntered(Node3D body)
    {
        if (body is Ball ball && !_hasBall)
        {
            _currentBall = ball;
            _hasBall = true;

            var ballMachine = _currentBall.GetNode<BallStateMachine>("StateMachine");
            var carriedState = ballMachine.GetNode<BallCarriedState>("Carried");
            carriedState.SetTarget(_dribblePoint);
            ballMachine.ChangeState("Carried");

            _player.StateMachine.ChangeState("Dribble");
        }
    }

    // ==========================================
    // ACCIONES DE BALÓN
    // ==========================================

    // Modificamos Kick para recibir el ratio de carga (de 0.0 a 1.0)
    public void Kick(float baseForce, float chargeRatio)
    {
        // Limitamos por seguridad para que nunca pase de 1.0 (100%)
        chargeRatio = Mathf.Clamp(chargeRatio, 0.0f, 1.0f);

        // Lerp calcula el valor exacto proporcional a la carga
        float finalForce = baseForce * Mathf.Lerp(MinKickForceMult, MaxKickForceMult, chargeRatio);
        float finalElevation = Mathf.Lerp(MinKickElevation, MaxKickElevation, chargeRatio);

        ExecuteBallRelease(finalForce, KickFriction, finalElevation); 
        GD.Print($"[BallHandler] Remate - Carga: {chargeRatio:P0} | Elevación: {finalElevation:F2} | Fuerza: {finalForce:F2}");
    }

    public void Pass()
    {
        float passForce = _player.Stats.ShootPower * PassForceMult;
        ExecuteBallRelease(passForce, PassFriction, PassElevation); 
    }

    private void ExecuteBallRelease(float force, float friction, float elevationAngle)
    {
        if (!_hasBall || _currentBall == null) return;

        var ballMachine = _currentBall.GetNode<BallStateMachine>("StateMachine");
        ballMachine.ChangeState("Free");

        Vector3 forwardDir = -_player.GetNode<Node3D>("Rotator").GlobalTransform.Basis.Z;
        Vector3 upDir = Vector3.Up * elevationAngle; 
        Vector3 finalDirection = (forwardDir + upDir).Normalized();

        _currentBall.LinearDamp = friction;
        _currentBall.ApplyCentralImpulse(finalDirection * force);

        _hasBall = false;
        _currentBall = null;
        _player.StateMachine.ChangeState("Move");
    }

    public bool HasBall() => _hasBall;
}
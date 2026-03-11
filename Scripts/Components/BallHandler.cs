using Godot;
using System;

public partial class BallHandler : Node
{
    private PlayerBase _player;
    private Area3D _grabArea;
    private Marker3D _dribblePoint;
    
    private Ball _currentBall;
    private bool _hasBall = false;

    public override void _Ready()
    {
        _player = GetOwner<PlayerBase>();
        _grabArea = _player.GetNode<Area3D>("Rotator/GrabArea");
        _dribblePoint = _player.GetNode<Marker3D>("Rotator/DribblePoint");

        _grabArea.BodyEntered += OnBallEntered;
    }

    private void OnBallEntered(Node3D body)
    {
        if (body is Ball ball && !_hasBall)
        {
            _currentBall = ball;
            _hasBall = true;

            // 1. Configurar Pelota
            var ballMachine = _currentBall.GetNode<BallStateMachine>("StateMachine");
            var carriedState = ballMachine.GetNode<BallCarriedState>("Carried");
            carriedState.SetTarget(_dribblePoint);
            ballMachine.ChangeState("Carried");

            // 2. Cambiar estado del Jugador a Dribble
            _player.StateMachine.ChangeState("Dribble");
            
            GD.Print($"[BallHandler] {_player.Name} tomó la pelota.");
        }
    }

    // Acción: TIRO (Tecla Z)
    public void Kick(float force)
    {
        ExecuteBallRelease(force, 0.5f); // Fricción baja para que ruede mucho
        GD.Print("[BallHandler] ¡Remate al arco!");
    }

    // Acción: PASE (Tecla X)
    public void Pass()
    {
        // El pase usa un 60% de la fuerza de tiro y más fricción para que frene
        float passForce = _player.Stats.ShootPower * 0.6f;
        ExecuteBallRelease(passForce, 3.0f); // Fricción alta para que se detenga cerca
        GD.Print("[BallHandler] Pase corto ejecutado.");
    }

    private void ExecuteBallRelease(float force, float friction)
    {
        if (!_hasBall || _currentBall == null) return;

        // 1. Liberar físicamente
        var ballMachine = _currentBall.GetNode<BallStateMachine>("StateMachine");
        ballMachine.ChangeState("Free");

        // 2. Aplicar Fuerza
        Vector3 dir = -_player.GetNode<Node3D>("Rotator").GlobalTransform.Basis.Z;
        _currentBall.LinearDamp = friction;
        _currentBall.ApplyCentralImpulse(dir * force);

        // 3. Limpiar estado
        _hasBall = false;
        _currentBall = null;
        _player.StateMachine.ChangeState("Move");
    }

    public bool HasBall() => _hasBall;
}
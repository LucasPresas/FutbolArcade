using Godot;
using System;
using System.Diagnostics;

public partial class AiController : Node, IController
{
    private PlayerBase _player;
    private Ball _ball;
    private Vector3 _homePosition;

    [Export] public float DetectionRange = 20.0f;
    [Export] public float ShootingRange = 14.0f;
    [Export] public float TackleDistance = 1.8f;

    // Opcional: un pequeño cooldown local para decisiones (además del de la StateMachine)
    [Export] public float DecisionCooldown = 0.08f;
    private double _lastDecisionTime = -9999;

    // Stopwatch compartido para medir tiempo con precisión sin depender de OS.*
    private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    // Helper de tiempo usando Stopwatch (compatible con .NET y Godot 4.6)
    private double NowSeconds() => _stopwatch.Elapsed.TotalSeconds;

    public override void _Ready()
    {
        _player = GetOwner<PlayerBase>();
        _ball = GetTree().CurrentScene.FindChild("Ball") as Ball;
        _homePosition = _player.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Llamamos al decisor en un intervalo corto para no saturar llamadas
        double now = NowSeconds();
        if (now - _lastDecisionTime >= DecisionCooldown)
        {
            DecideState();
            _lastDecisionTime = now;
        }
    }

    // Loop de decisión con prioridades claras
    private void DecideState()
    {
        if (_player == null || _player.StateMachine == null) return;

        // Prioridad: Tackle > Shoot > Charge(Dribble) > ChaseBall > ReturnHome
        string current = _player.StateMachine.CurrentStateName;

        if (IsTackling())
        {
            if (current != "tackle") _player.StateMachine.ChangeState("tackle");
            return;
        }

        if (IsShooting())
        {
            if (current != "shoot") _player.StateMachine.ChangeState("shoot");
            return;
        }

        if (IsCharging())
        {
            // algunos proyectos usan "dribble" o "charge" como nombre; ajustá si es necesario
            if (current != "dribble" && current != "charge") _player.StateMachine.ChangeState("dribble");
            return;
        }

        // Si la pelota está en rango de detección, perseguirla
        if (_ball != null)
        {
            float distToBall = _player.GlobalPosition.DistanceTo(_ball.GlobalPosition);
            if (distToBall < DetectionRange)
            {
                // Ajustá el nombre del estado de persecución según tu escena (chase, chaseball, pursue, etc.)
                if (current != "chase" && current != "pursue" && current != "chaseball")
                    _player.StateMachine.ChangeState("chase");
                return;
            }
        }

        // Por defecto volver a home/idle
        if (current != "idle" && current != "return") _player.StateMachine.ChangeState("idle");
    }

    public Vector3 GetMoveDirection()
    {
        if (_ball == null || _player.TargetGoal == null) return Vector3.Zero;

        if (_player.BallHandler.HasBall())
        {
            if (IsCharging()) return Vector3.Zero;
            float distToGoal = _player.GlobalPosition.DistanceTo(_player.TargetGoal.GlobalPosition);
            if (distToGoal < 4.0f) return Vector3.Zero;
            return GetDirectionTo(_player.TargetGoal.GlobalPosition);
        }

        float distToBall = _player.GlobalPosition.DistanceTo(_ball.GlobalPosition);
        if (distToBall < DetectionRange)
        {
            Vector3 dir = GetDirectionTo(_ball.GlobalPosition);

            // --- ANTI-MISIL: Desaceleración por proximidad ---
            float proximityScale = Mathf.Clamp(distToBall / 2.5f, 0.3f, 1.0f);
            return dir * proximityScale;
        }

        return GetDirectionTo(_homePosition);
    }

    public bool IsCharging()
    {
        if (!_player.BallHandler.HasBall() || _player.TargetGoal == null) return false;

        float distToGoal = _player.GlobalPosition.DistanceTo(_player.TargetGoal.GlobalPosition);
        if (distToGoal > ShootingRange) return false;

        // Potencia dinámica: a 3m (0.2), a 14m (0.85)
        float desiredCharge = Mathf.Remap(distToGoal, 3.0f, ShootingRange, 0.2f, 0.85f);
        desiredCharge = Mathf.Clamp(desiredCharge, 0.15f, 0.9f);

        var dribble = _player.StateMachine.GetNodeOrNull<PlayerDribbleState>("Dribble");
        if (dribble == null) return false;

        return dribble.GetCurrentChargeRatio() < desiredCharge;
    }

    public bool IsShooting() => !IsCharging() && _player.BallHandler.HasBall();

    public bool IsTackling()
    {
        if (_ball == null || _player.BallHandler.HasBall()) return false;
        float dist = _player.GlobalPosition.DistanceTo(_ball.GlobalPosition);
        Vector3 forward = -_player.GetNode<Node3D>("Rotator").GlobalTransform.Basis.Z;
        Vector3 toBall = (_ball.GlobalPosition - _player.GlobalPosition).Normalized();
        return dist < TackleDistance && forward.Dot(toBall) > 0.8f;
    }

    public bool IsPassing() => false;

    private Vector3 GetDirectionTo(Vector3 target)
    {
        Vector3 dir = (target - _player.GlobalPosition);
        return new Vector3(dir.X, 0, dir.Z).Normalized();
    }
}

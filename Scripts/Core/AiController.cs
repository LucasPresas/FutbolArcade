using Godot;
using System;

public partial class AiController : Node, IController
{
    private PlayerBase _player;
    private Ball _ball;
    private Vector3 _homePosition;

    [Export] public float DetectionRange = 20.0f;
    [Export] public float ShootingRange = 14.0f;
    [Export] public float TackleDistance = 1.8f;

    public override void _Ready()
    {
        _player = GetOwner<PlayerBase>();
        _ball = GetTree().CurrentScene.FindChild("Ball") as Ball;
        _homePosition = _player.GlobalPosition;
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
        if (distToBall < DetectionRange) {
            Vector3 dir = GetDirectionTo(_ball.GlobalPosition);
            
            // --- ANTI-MISIL: Desaceleración por proximidad ---
            // Si está a 2m o menos de la pelota, el vector de dirección se reduce
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

    private Vector3 GetDirectionTo(Vector3 target) {
        Vector3 dir = (target - _player.GlobalPosition);
        return new Vector3(dir.X, 0, dir.Z).Normalized();
    }
}
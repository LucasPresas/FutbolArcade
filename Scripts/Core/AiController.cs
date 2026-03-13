using Godot;
using System;

public partial class AiController : Node, IController
{
    private PlayerBase _player;
    private Ball _ball;
    private Vector3 _homePosition;

    [Export] public float DetectionRange = 20.0f;
    [Export] public float ShootingRange = 12.0f;
    [Export] public float TackleDistance = 1.8f; // Reducido para mayor precisión

    public override void _Ready()
    {
        _player = GetOwner<PlayerBase>();
        _ball = GetTree().CurrentScene.FindChild("Ball") as Ball;
        _homePosition = _player.GlobalPosition;
    }

    public Vector3 GetMoveDirection()
    {
        if (_ball == null || _player.TargetGoal == null || _player.MyGoal == null) return Vector3.Zero;

        if (_player.BallHandler.HasBall()) return GetDirectionTo(_player.TargetGoal.GlobalPosition);

        float distToBall = _player.GlobalPosition.DistanceTo(_ball.GlobalPosition);
        float ballDistToMyGoal = _ball.GlobalPosition.DistanceTo(_player.MyGoal.GlobalPosition);

        if (ballDistToMyGoal < 10.0f) return GetDirectionTo(_ball.GlobalPosition);

        if (distToBall < DetectionRange)
        {
            // Predicción leve
            Vector3 predictedPos = _ball.GlobalPosition + (_ball.LinearVelocity * 0.1f);
            return GetDirectionTo(predictedPos);
        }

        return GetDirectionTo(_homePosition);
    }

    public bool IsTackling()
    {
        if (_ball == null || _player.BallHandler.HasBall()) return false;
        
        float dist = _player.GlobalPosition.DistanceTo(_ball.GlobalPosition);
        
        // NUEVO: Solo taclea si la pelota está en frente (Dot Product)
        Vector3 forward = -_player.GetNode<Node3D>("Rotator").GlobalTransform.Basis.Z;
        Vector3 toBall = (_ball.GlobalPosition - _player.GlobalPosition).Normalized();
        float dot = forward.Dot(toBall);

        // Taclea solo si está cerca Y mirando a la pelota
        return dist < TackleDistance && dot > 0.8f;
    }

    public bool IsShooting() => false;

    public bool IsCharging()
    {
        if (!_player.BallHandler.HasBall() || _player.TargetGoal == null) return false;
        
        float dist = _player.GlobalPosition.DistanceTo(_player.TargetGoal.GlobalPosition);
        Vector3 forward = -_player.GetNode<Node3D>("Rotator").GlobalTransform.Basis.Z;
        Vector3 toGoal = (_player.TargetGoal.GlobalPosition - _player.GlobalPosition).Normalized();
        
        return dist < ShootingRange && forward.Dot(toGoal) > 0.7f;
    }

    public bool IsPassing() => false;

    private Vector3 GetDirectionTo(Vector3 target)
    {
        Vector3 dir = (target - _player.GlobalPosition);
        return new Vector3(dir.X, 0, dir.Z).Normalized();
    }
}
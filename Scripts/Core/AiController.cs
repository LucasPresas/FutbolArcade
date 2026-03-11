using Godot;

[GlobalClass]
public partial class AiController : Node, IController
{
    private PlayerBase _player;
    private Ball _ball;
    [Export] public float ShootingDistance = 12.0f;

    public override void _Ready()
    {
        _player = GetParent().GetParent<PlayerBase>(); // Sube desde 'Controllers' al padre
        _ball = GetTree().GetFirstNodeInGroup("Ball") as Ball;
    }

    public Vector3 GetMoveDirection()
    {
        if (_ball == null) return Vector3.Zero;
        Vector3 target = _player.BallHandler.HasBall() ? _player.TargetGoal.GlobalPosition : _ball.GlobalPosition;
        Vector3 dir = target - _player.GlobalPosition;
        dir.Y = 0;
        return dir.Normalized();
    }

    public bool IsShooting() => _player.BallHandler.HasBall() && _player.GlobalPosition.DistanceTo(_player.TargetGoal.GlobalPosition) < ShootingDistance;
    public bool IsPassing() => false;
    public bool IsTackling() => false;
    public bool IsCharging() => false;
}
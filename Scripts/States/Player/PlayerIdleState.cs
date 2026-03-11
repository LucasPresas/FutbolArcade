using Godot;

public partial class PlayerIdleState : State
{
    private PlayerBase _player;
    private PlayerStateMachine _machine;

    public override void _Ready()
    {
        _player = GetOwner<PlayerBase>();
        _machine = GetParent<PlayerStateMachine>();
    }

    public override void PhysicsUpdate(float delta)
    {
        if (_player.Controller.GetMoveDirection() != Vector3.Zero)
        {
            _machine.ChangeState("Move");
            return;
        }

        Vector3 vel = _player.Velocity;
        vel.X = Mathf.MoveToward(vel.X, 0, _player.Stats.Acceleration * delta);
        vel.Z = Mathf.MoveToward(vel.Z, 0, _player.Stats.Acceleration * delta);
        _player.Velocity = vel;
        _player.MoveAndSlide();
    }
}
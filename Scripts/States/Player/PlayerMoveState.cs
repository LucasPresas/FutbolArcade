using Godot;

public partial class PlayerMoveState : State
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
        Vector3 dir = _player.Controller.GetMoveDirection();
        
        if (dir == Vector3.Zero)
        {
            _machine.ChangeState("Idle");
            return;
        }

        // Movimiento
        Vector3 vel = _player.Velocity;
        vel.X = dir.X * _player.Stats.Speed;
        vel.Z = dir.Z * _player.Stats.Speed;
        _player.Velocity = vel;
        _player.MoveAndSlide();

        // Rotación
        Node3D rotator = _player.GetNode<Node3D>("Rotator");
        float targetAngle = Mathf.Atan2(-dir.X, -dir.Z);
        rotator.Rotation = new Vector3(0, (float)Mathf.LerpAngle(rotator.Rotation.Y, targetAngle, _player.Stats.RotationSpeed * delta), 0);
    }
}
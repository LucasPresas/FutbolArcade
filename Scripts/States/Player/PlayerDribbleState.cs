using Godot;

public partial class PlayerDribbleState : State
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
        
        // Movimiento (podés usar Stats.Speed o multiplicarlo por 0.9f para penalizar peso)
        Vector3 vel = _player.Velocity;
        vel.X = dir.X * _player.Stats.Speed;
        vel.Z = dir.Z * _player.Stats.Speed;
        _player.Velocity = vel;
        _player.MoveAndSlide();

        // Rotación siempre hacia donde apunta el joystick
        if (dir != Vector3.Zero)
        {
            Node3D rotator = _player.GetNode<Node3D>("Rotator");
            float targetAngle = Mathf.Atan2(-dir.X, -dir.Z);
            rotator.Rotation = new Vector3(0, (float)Mathf.LerpAngle(rotator.Rotation.Y, targetAngle, _player.Stats.RotationSpeed * delta), 0);
        }

        // Escuchamos el disparo
        if (_player.Controller.IsShooting())
        {
            _player.BallHandler.Kick(_player.Stats.ShootPower);
            _machine.ChangeState("Move");
        }
    }
}
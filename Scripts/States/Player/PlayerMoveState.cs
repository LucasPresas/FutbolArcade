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
        Vector3 currentVel = _player.Velocity;

        // 1. APLICAR GRAVEDAD (Evita que salgan volando)
        if (!_player.IsOnFloor())
        {
            currentVel.Y -= 9.8f * delta;
        }
        else
        {
            // Pequeña fuerza hacia abajo para mantener contacto con el suelo
            currentVel.Y = -0.1f; 
        }

        // 2. PROCESAR MOVIMIENTO (Con Lerp para suavizar impactos y frenado)
        // Si no hay input, desaceleramos en XZ pero mantenemos la Y
        float targetX = dir.X * _player.Stats.Speed;
        float targetZ = dir.Z * _player.Stats.Speed;

        currentVel.X = Mathf.Lerp(currentVel.X, targetX, delta * 10.0f);
        currentVel.Z = Mathf.Lerp(currentVel.Z, targetZ, delta * 10.0f);

        _player.Velocity = currentVel;
        _player.MoveAndSlide();

        // 3. CAMBIO A IDLE (Solo si estamos casi frenados y no hay input)
        if (dir == Vector3.Zero && _player.Velocity.Length() < 0.5f)
        {
            _machine.ChangeState("Idle");
            return;
        }

        // 4. ROTACIÓN
        if (dir != Vector3.Zero)
        {
            Node3D rotator = _player.GetNode<Node3D>("Rotator");
            float targetAngle = Mathf.Atan2(-dir.X, -dir.Z);
            rotator.Rotation = new Vector3(0, (float)Mathf.LerpAngle(rotator.Rotation.Y, targetAngle, _player.Stats.RotationSpeed * delta), 0);
        }

        // 5. LÓGICA DE TACKLE
        if (_player.Controller.IsTackling())
        {
            _player.BallHandler.Tackle();
        }
    }
}
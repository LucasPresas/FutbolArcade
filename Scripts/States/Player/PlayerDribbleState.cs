using Godot;

public partial class PlayerDribbleState : State
{
    private PlayerBase _player;
    private PlayerStateMachine _machine;

    // Variables para controlar la carga del tiro
    [Export] public float MaxChargeTime = 1.0f; // 1 segundo = 100% de fuerza
    private float _currentCharge = 0f;
    private bool _isCharging = false;

    public override void _Ready()
    {
        _player = GetOwner<PlayerBase>();
        _machine = GetParent<PlayerStateMachine>();
    }

    public override void PhysicsUpdate(float delta)
    {
        Vector3 dir = _player.Controller.GetMoveDirection();
        
        // Penalizamos la velocidad si está cargando el tiro (50% de velocidad)
        float currentSpeed = _isCharging ? (_player.Stats.Speed * 0.5f) : _player.Stats.Speed;
        
        Vector3 vel = _player.Velocity;
        vel.X = dir.X * currentSpeed;
        vel.Z = dir.Z * currentSpeed;
        _player.Velocity = vel;
        _player.MoveAndSlide();

        // Rotación siempre hacia donde apunta el joystick
        if (dir != Vector3.Zero)
        {
            Node3D rotator = _player.GetNode<Node3D>("Rotator");
            float targetAngle = Mathf.Atan2(-dir.X, -dir.Z);
            rotator.Rotation = new Vector3(
                0, 
                (float)Mathf.LerpAngle(rotator.Rotation.Y, targetAngle, _player.Stats.RotationSpeed * delta), 
                0
            );
        }

        // ==========================================
        // LÓGICA DE CARGA DE TIRO Y UI
        // ==========================================
        
        // 1. Si mantiene presionado, sumamos tiempo
        if (_player.Controller.IsCharging())
        {
            _isCharging = true;
            _currentCharge += delta;
            
            // Limitamos para que la carga no pase de MaxChargeTime (ej. 1 segundo máximo)
            _currentCharge = Mathf.Min(_currentCharge, MaxChargeTime);

            // Mostramos y actualizamos el texto visual
            if (_player.ChargeLabel != null)
            {
                _player.ChargeLabel.Visible = true;
                
                // Calculamos el porcentaje (0 a 100) y lo convertimos a int para que no tenga decimales
                int percent = (int)((_currentCharge / MaxChargeTime) * 100f);
                
                // Actualizamos el texto
                _player.ChargeLabel.Text = $"{percent}%";
            }
        }
        // 2. Si soltó el botón Y estaba cargando, ejecuta el remate
        else if (_isCharging) 
        {
            // Calculamos el porcentaje de carga (de 0.0 a 1.0)
            float chargeRatio = _currentCharge / MaxChargeTime;
            
            // Llamamos al Kick dinámico del BallHandler
            _player.BallHandler.Kick(_player.Stats.ShootPower, chargeRatio);
            
            // Apagamos el texto visual
            if (_player.ChargeLabel != null)
            {
                _player.ChargeLabel.Visible = false;
                _player.ChargeLabel.Text = "0%";
            }

            // Limpiamos las variables para el próximo tiro
            _isCharging = false;
            _currentCharge = 0f;
            
            _machine.ChangeState("Move");
        }

        // ==========================================
        // LÓGICA DE PASE
        // ==========================================
        // Solo permite pasar si no está en medio de cargar un tiro
        if (!_isCharging && _player.Controller.IsPassing())
        {
            _player.BallHandler.Pass();
            _machine.ChangeState("Move");
        }
    }

    // ==========================================
    // SEGURO ANTI-BUGS
    // ==========================================
    // Si el jugador cambia de estado repentinamente (ej. le roban la pelota o hacen un gol), 
    // nos aseguramos de resetear la carga y ocultar la UI.
    public override void Exit()
    {
        _isCharging = false;
        _currentCharge = 0f;

        if (_player != null && _player.ChargeLabel != null)
        {
            _player.ChargeLabel.Visible = false;
            _player.ChargeLabel.Text = "0%";
        }
    }
}
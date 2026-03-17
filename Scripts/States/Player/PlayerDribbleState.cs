using Godot;

public partial class PlayerDribbleState : State
{
    private PlayerBase _player;
    private PlayerStateMachine _machine;

    [Export] public float MaxChargeTime = 1.0f;
    private float _currentCharge = 0f;
    private bool _isCharging = false;

    public override void _Ready()
    {
        _player = GetOwner<PlayerBase>();
        _machine = GetParent<PlayerStateMachine>();
    }

    public float GetCurrentChargeRatio() => _currentCharge / MaxChargeTime;

    public override void PhysicsUpdate(float delta)
    {
        Vector3 dir = _player.Controller.GetMoveDirection();
        
        // Freno total (0) si está cargando, o velocidad según el input
        float targetSpeed = _isCharging ? 0.0f : (_player.Stats.Speed * dir.Length());
        
        Vector3 vel = _player.Velocity;
        // Aumentamos a 20.0f para un frenado mucho más seco
        vel.X = Mathf.Lerp(vel.X, dir.X * targetSpeed, delta * 20.0f);
        vel.Z = Mathf.Lerp(vel.Z, dir.Z * targetSpeed, delta * 20.0f);
        _player.Velocity = vel;
        _player.MoveAndSlide();

        if (dir != Vector3.Zero || _isCharging)
        {
            Node3D rotator = _player.GetNode<Node3D>("Rotator");
            float targetAngle;
            
            if (_isCharging && _player.TargetGoal != null) {
                Vector3 toGoal = (_player.TargetGoal.GlobalPosition - _player.GlobalPosition).Normalized();
                targetAngle = Mathf.Atan2(-toGoal.X, -toGoal.Z);
            } else {
                targetAngle = Mathf.Atan2(-dir.X, -dir.Z);
            }
            rotator.Rotation = new Vector3(0, (float)Mathf.LerpAngle(rotator.Rotation.Y, targetAngle, _player.Stats.RotationSpeed * delta), 0);
        }

        if (_player.Controller.IsCharging())
        {
            _isCharging = true;
            _currentCharge += delta;
            _currentCharge = Mathf.Min(_currentCharge, MaxChargeTime);
            UpdateUI();
        }
        else if (_isCharging || _player.Controller.IsShooting())
        {
            ExecuteKick();
            return; 
        }

        if (!_isCharging && _player.Controller.IsPassing())
        {
            _player.BallHandler.Pass();
            _machine.ChangeState("Move");
        }
    }

    private void ExecuteKick()
    {
        float chargeRatio = GetCurrentChargeRatio();
        _player.BallHandler.Kick(_player.Stats.ShootPower, chargeRatio);
        ResetChargeUI();
        _isCharging = false;
        _currentCharge = 0f;
        _machine.ChangeState("Move");
    }

    private void UpdateUI()
    {
        if (_player.ChargeLabel != null) {
            _player.ChargeLabel.Visible = true;
            _player.ChargeLabel.Text = $"{(int)(GetCurrentChargeRatio() * 100f)}%";
        }
    }

    private void ResetChargeUI() {
        if (_player.ChargeLabel != null) _player.ChargeLabel.Visible = false;
    }

    public override void Exit() { _isCharging = false; _currentCharge = 0f; ResetChargeUI(); }
}
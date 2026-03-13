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

    public override void PhysicsUpdate(float delta)
    {
        Vector3 dir = _player.Controller.GetMoveDirection();
        float currentSpeed = _isCharging ? (_player.Stats.Speed * 0.5f) : _player.Stats.Speed;
        
        Vector3 vel = _player.Velocity;
        vel.X = Mathf.Lerp(vel.X, dir.X * currentSpeed, delta * 10.0f);
        vel.Z = Mathf.Lerp(vel.Z, dir.Z * currentSpeed, delta * 10.0f);
        _player.Velocity = vel;
        _player.MoveAndSlide();

        if (dir != Vector3.Zero)
        {
            Node3D rotator = _player.GetNode<Node3D>("Rotator");
            float targetAngle = Mathf.Atan2(-dir.X, -dir.Z);
            rotator.Rotation = new Vector3(0, (float)Mathf.LerpAngle(rotator.Rotation.Y, targetAngle, _player.Stats.RotationSpeed * delta), 0);
        }

        if (_player.Controller.IsCharging())
        {
            _isCharging = true;
            _currentCharge += delta;
            _currentCharge = Mathf.Min(_currentCharge, MaxChargeTime);

            if (_player.ChargeLabel != null)
            {
                _player.ChargeLabel.Visible = true;
                int percent = (int)((_currentCharge / MaxChargeTime) * 100f);
                _player.ChargeLabel.Text = $"{percent}%";
            }

            // AUTO-KICK IA
            if (!_player.IsUserControlled && _currentCharge >= 0.35f) ExecuteKick();
        }
        else if (_isCharging) 
        {
            ExecuteKick();
        }

        if (!_isCharging && _player.Controller.IsPassing())
        {
            _player.BallHandler.Pass();
            _machine.ChangeState("Move");
        }
    }

    private void ExecuteKick()
    {
        float chargeRatio = _currentCharge / MaxChargeTime;
        _player.BallHandler.Kick(_player.Stats.ShootPower, chargeRatio);
        ResetChargeUI();
        _isCharging = false;
        _currentCharge = 0f;
        _machine.ChangeState("Move");
    }

    private void ResetChargeUI()
    {
        if (_player.ChargeLabel != null) { _player.ChargeLabel.Visible = false; _player.ChargeLabel.Text = "0%"; }
    }

    public override void Exit() { _isCharging = false; _currentCharge = 0f; ResetChargeUI(); }
}
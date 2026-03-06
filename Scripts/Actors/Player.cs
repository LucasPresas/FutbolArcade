using Godot;

public partial class Player : ActorBase
{
    [Export] public Area3D KickPicker;
    [Export] public Area3D PassScanArea; // Detects nearby teammates for auto-pass

    // --- Power Bar (3D visual above player) ---
    private MeshInstance3D _powerBarFill;
    private Node3D _powerBarRoot;

    // --- Shot charging ---
    private float _chargeAmount = 0f;
    private bool _isCharging = false;
    private const float ChargeSpeed = 1.2f; // 0→1 in ~0.83s

    private BallHandlerComponent _ballHandler;

    // --- Charged Pass ---
    private float _passCharge = 0f;
    private bool _isChargingPass = false;
    private float _passCooldown = 0f;
    private float _passHoldTime = 0f;
    private const float PassChargeSpeed = 1.5f;
    private const float PassCooldownTime = 1.0f;
    private const float PassHoldThreshold = 0.2f; // tap below this = quick pass, hold above = charged + slow-mo
    private const float PassSlowMotionScale = 0.2f;
    private const float PassZoomSize = 20f;

    // --- Tackle window ---
    private bool _isTackling = false;
    private float _tackleWindow = 0f;
    private const float TackleWindowTime = 0.25f;

    private MatchCamera _matchCamera;
    private float _cameraOriginalSize = 35f;
    private Label _passChargeLabel;
    private Label _shootLabel;
    private Node3D _selectionRing;

    public override void _Ready()
    {
        base._Ready();

        _ballHandler = GetNodeOrNull<BallHandlerComponent>("Components/BallHandler");
        _powerBarRoot = GetNodeOrNull<Node3D>("PowerBar");
        _powerBarFill = GetNodeOrNull<MeshInstance3D>("PowerBar/Fill");

        if (_powerBarRoot != null) _powerBarRoot.Visible = false;

        if (Controller == null && HasNode("Controllers/PlayerInputController"))
            SetController(GetNode<IController>("Controllers/PlayerInputController"));

        _matchCamera = GetTree().Root.GetNodeOrNull<MatchCamera>("Pitch/Camera3D")
                    ?? GetParent()?.GetNodeOrNull<MatchCamera>("Camera3D");

        _passChargeLabel = GetParent()?.GetNodeOrNull<Label>("UI/PassChargeLabel");
        _shootLabel      = GetParent()?.GetNodeOrNull<Label>("UI/ShootLabel");

        _selectionRing = GetNodeOrNull<Node3D>("SelectionRing");
        if (_selectionRing != null) _selectionRing.Visible = Controller is PlayerInputController;
    }

    public override void _Process(double delta)
    {
        HandleShootCharge(delta);
        HandlePassCharge(delta);
        HandleAIActions();
        HandleTackle(delta);
    }

    // Steals the ball from a nearby opponent who is carrying it
    // Opens a 0.25s window so the hit detection doesn't need to be frame-perfect
    private void HandleTackle(double delta)
    {
        if (Controller == null) return;

        if (Controller.IsTackling())
        {
            _isTackling = true;
            _tackleWindow = TackleWindowTime;
        }

        if (!_isTackling) return;

        _tackleWindow -= (float)delta;
        if (_tackleWindow <= 0f) { _isTackling = false; return; }
        if (KickPicker == null) return;

        foreach (var body in KickPicker.GetOverlappingBodies())
        {
            if (body is Player opponent && opponent != this)
            {
                var opponentHandler = opponent.GetNodeOrNull<BallHandlerComponent>("Components/BallHandler");
                Ball stolenBall = opponentHandler?.CarriedBall;
                if (stolenBall == null) continue;

                opponentHandler.ReleaseBall();
                stolenBall.StateMachine?.TransitionTo("Free");

                Vector3 dir = (GlobalPosition - opponent.GlobalPosition);
                dir.Y = 0;
                if (dir.LengthSquared() > 0.01f)
                    stolenBall.ApplyCentralImpulse(dir.Normalized() * 2f);

                GD.Print($"[Tackle] {Name} robo la pelota de {opponent.Name}");
                _isTackling = false;
                break;
            }
        }
    }

    // Triggered every _Process frame for AI-controlled players
    private void HandleAIActions()
    {
        if (Controller is PlayerInputController || Controller == null) return;

        if (Controller.IsShooting())
        {
            float force = StatsResource != null ? StatsResource.ShootForce : 12f;
            TryAction(force, Vector3.Zero);
        }
    }

    private void HandleShootCharge(double delta)
    {
        // Only human-controlled players read keyboard input
        if (Controller is not PlayerInputController && Controller != null) return;

        if (Input.IsActionPressed("action_shoot"))
        {
            _isCharging = true;
            _chargeAmount = Mathf.Min(1f, _chargeAmount + ChargeSpeed * (float)delta);

            if (_powerBarRoot != null) _powerBarRoot.Visible = true;
            if (_shootLabel != null)
            {
                _shootLabel.Visible = true;
                _shootLabel.Text = $"Disparo: {Mathf.RoundToInt(_chargeAmount * 100)}%";
            }
            if (_matchCamera != null) _matchCamera.RequestZoomOn(this);
            UpdatePowerBarFill(_chargeAmount);
        }

        if (Input.IsActionJustReleased("action_shoot") && _isCharging)
        {
            float minForce = StatsResource != null ? StatsResource.ShootForce * 0.3f : 3f;
            float maxForce = StatsResource != null ? StatsResource.ShootForce : 12f;
            float finalForce = Mathf.Lerp(minForce, maxForce, _chargeAmount);

            Vector3 arc = new Vector3(0, 0.05f * _chargeAmount, 0);
            TryAction(finalForce, arc);

            _chargeAmount = 0f;
            _isCharging = false;
            if (_powerBarRoot != null) _powerBarRoot.Visible = false;
            if (_shootLabel != null) _shootLabel.Visible = false;
            if (_matchCamera != null) _matchCamera.CancelZoom();
        }
    }

    private void HandlePassCharge(double delta)
    {
        if (_passCooldown > 0f) { _passCooldown -= (float)delta; return; }
        if (Controller is not PlayerInputController && Controller != null) return;

        if (Input.IsActionJustPressed("action_pass") && !_isChargingPass)
        {
            _isChargingPass = true;
            _passCharge = 0f;
            _passHoldTime = 0f;
            // Slow-mo is NOT activated yet — wait for hold threshold
        }

        if (_isChargingPass)
        {
            // Accumulate real-world hold time (before slow-mo, delta is still 1x)
            _passHoldTime += (float)delta;

            // Activate slow-mo only once the player holds past the threshold
            if (_passHoldTime >= PassHoldThreshold && Engine.TimeScale > PassSlowMotionScale)
            {
                Engine.TimeScale = PassSlowMotionScale;
                if (_passChargeLabel != null) _passChargeLabel.Visible = true;
            }

            // Accumulate charge only while in slow-mo
            if (Engine.TimeScale < 1.0f)
            {
                float realDelta = (float)delta / Mathf.Max((float)Engine.TimeScale, 0.001f);
                _passCharge = Mathf.Min(1f, _passCharge + PassChargeSpeed * realDelta);
                if (_passChargeLabel != null)
                    _passChargeLabel.Text = $"Pase: {Mathf.RoundToInt(_passCharge * 100)}%";
                if (_matchCamera != null)
                    _matchCamera.RequestZoomOn(this);
            }
        }

        if (Input.IsActionJustReleased("action_pass") && _isChargingPass)
        {
            if (_passHoldTime < PassHoldThreshold)
            {
                // Quick tap → instant pass at base force, no slow-mo to undo
                float passForce = StatsResource != null ? StatsResource.PassForce : 6f;
                Player nearestTeammate = FindNearestTeammate();
                if (nearestTeammate != null)
                    TryPassToTarget(nearestTeammate.GlobalPosition, passForce);
                else
                    TryAction(passForce, Vector3.Zero);
                _isChargingPass = false;
                _passCharge = 0f;
                _passCooldown = 0.3f;
            }
            else
            {
                // Charged release → fire with accumulated force
                ExecuteChargedPass(_passCharge);
                ResetPassCharge();
            }
        }
    }

    private void ExecuteChargedPass(float charge)
    {
        Player nearestTeammate = FindNearestTeammate();
        float minForce = StatsResource != null ? StatsResource.PassForce * 0.3f : 2f;
        float maxForce = StatsResource != null ? StatsResource.PassForce : 6f;
        float finalForce = Mathf.Lerp(minForce, maxForce, charge);

        if (nearestTeammate != null)
            TryPassToTarget(nearestTeammate.GlobalPosition, finalForce);
        else
            TryAction(finalForce, Vector3.Zero);
    }

    private void ResetPassCharge()
    {
        _isChargingPass = false;
        _passCharge = 0f;
        _passCooldown = PassCooldownTime;
        Engine.TimeScale = 1.0f;

        if (_passChargeLabel != null) _passChargeLabel.Visible = false;
        if (_matchCamera != null) _matchCamera.CancelZoom();
    }

    private Player FindNearestTeammate()
    {
        if (PassScanArea == null) return null;

        Player nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var body in PassScanArea.GetOverlappingBodies())
        {
            if (body is Player teammate && teammate != this)
            {
                float dist = GlobalPosition.DistanceTo(teammate.GlobalPosition);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = teammate;
                }
            }
        }

        return nearest;
    }

    private void TryPassToTarget(Vector3 targetPos, float passForce)
    {
        Ball ball = _ballHandler?.CarriedBall;
        if (ball == null) return;

        Vector3 direction = (targetPos - GlobalPosition).Normalized();
        direction.Y = 0;

        _ballHandler?.ReleaseBall();
        ball.StateMachine?.TransitionTo("Free");
        ball.ReceiveKick(direction, passForce);

        GD.Print($"[Player] Pase cargado ({Mathf.RoundToInt(passForce * 10) / 10f}N) hacia {targetPos}");
    }

    private void UpdatePowerBarFill(float charge)
    {
        if (_powerBarFill == null) return;
        // Scale fill on X axis from 0 to 1
        _powerBarFill.Scale = new Vector3(charge, 1f, 1f);
        // Color goes from green → yellow → red
        if (_powerBarFill.GetSurfaceOverrideMaterial(0) is StandardMaterial3D mat)
        {
            mat.AlbedoColor = new Color(charge, 1f - charge * 0.8f, 0f);
        }
    }

    public void TryAction(float force, Vector3 arc)
    {
        Ball ball = _ballHandler?.CarriedBall;

        if (ball == null && KickPicker != null)
        {
            foreach (var body in KickPicker.GetOverlappingBodies())
            {
                if (body is Ball b) { ball = b; break; }
            }
        }

        if (ball == null) return;

        Vector3 lookDir = Visuals != null
            ? -Visuals.GlobalTransform.Basis.Z
            : -GlobalTransform.Basis.Z;
        Vector3 finalDir = (lookDir + arc).Normalized();

        _ballHandler?.ReleaseBall();
        ball.StateMachine?.TransitionTo("Free");
        ball.ReceiveKick(finalDir, force);
    }
}

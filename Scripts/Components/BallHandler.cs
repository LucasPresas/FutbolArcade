 using Godot;

using System;


public partial class BallHandler : Node

{

    private PlayerBase _player;

    private Area3D _grabArea;

    private Marker3D _dribblePoint;

   

    // Eliminamos la búsqueda por texto y usamos directamente el Export

    [ExportGroup("Nodos de Referencia")]

    [Export] public Area3D TackleArea; // Arrastrá el Area3D aquí en el Inspector


    private Ball _currentBall;

    private bool _hasBall = false;


    [ExportGroup("Físicas de Tiro")]

    [Export] public float MinKickElevation = 0.05f;

    [Export] public float MaxKickElevation = 0.4f;  

    [Export] public float MinKickForceMult = 0.6f;  

    [Export] public float MaxKickForceMult = 1.3f;  

    [Export] public float KickFriction = 0.5f;      


    [ExportGroup("Físicas de Pase")]

    [Export] public float PassElevation = 0.02f;    

    [Export] public float PassForceMult = 0.6f;

    [Export] public float PassFriction = 2.5f;      


    [ExportGroup("Configuración de Tackle")]

    [Export] public Color TackleVisualColor = new Color(1, 1, 0, 0.5f);

    [Export] public float TackleImpulse = 10.0f;

    [Export] public float PushForce = 15.0f;    


    public override void _Ready()

    {

        _player = GetOwner<PlayerBase>();

        _grabArea = _player.GetNode<Area3D>("Rotator/GrabArea");

        _dribblePoint = _player.GetNode<Marker3D>("Rotator/DribblePoint");


        // Ya no necesitamos GetNode para TackleArea porque viene del Export

        _grabArea.BodyEntered += OnBallEntered;


        if (TackleArea == null)

        {

            GD.PrintErr($"[BallHandler] {GetOwner().Name}: ¡No asignaste el TackleArea en el Inspector!");

        }

    }


    // ... (HandleGoalScored y OnBallEntered igual) ...


    public override void _EnterTree() { Goal.OnGoalScored += HandleGoalScored; }

    public override void _ExitTree() { Goal.OnGoalScored -= HandleGoalScored; }


    private void HandleGoalScored(string scorerTeam)

    {

        if (_hasBall) { _hasBall = false; _currentBall = null; _player.StateMachine.ChangeState("Idle"); }

    }


    private void OnBallEntered(Node3D body)

    {

        if (body is Ball ball && !_hasBall)

        {

            _currentBall = ball;

            _hasBall = true;

            var ballMachine = _currentBall.GetNode<BallStateMachine>("StateMachine");

            var carriedState = ballMachine.GetNode<BallCarriedState>("Carried");

            carriedState.SetTarget(_dribblePoint);

            ballMachine.ChangeState("Carried");

            _player.StateMachine.ChangeState("Dribble");

        }

    }


    public void Tackle()

    {

        if (_hasBall) return;


        GD.Print($"[TACKLE]: {_player.Name} se lanza.");

        ShowActionVisual(TackleVisualColor);


        // 1. IMPULSO (DASH) HACIA ADELANTE

        Vector3 forward = -_player.GetNode<Node3D>("Rotator").GlobalTransform.Basis.Z;

        _player.Velocity += forward * TackleImpulse;


        // Usamos la variable exportada directamente

        if (TackleArea == null) return;


        var bodies = TackleArea.GetOverlappingBodies();

        foreach (var body in bodies)

        {

            if (body is PlayerBase opponent && opponent != _player)

            {

                Vector3 pushDir = (opponent.GlobalPosition - _player.GlobalPosition).Normalized();

                pushDir.Y = 0.2f;

                opponent.Velocity += pushDir * PushForce;

                GD.Print($"[TACKLE]: {opponent.Name} fue empujado!");

            }


            if (body is Ball ball)

            {

                OnBallEntered(ball);

                break;

            }

        }

    }


    // ... (Kick, Pass, ExecuteBallRelease y ShowActionVisual igual) ...


    public void Kick(float baseForce, float chargeRatio)

    {

        chargeRatio = Mathf.Clamp(chargeRatio, 0.0f, 1.0f);

        float finalForce = baseForce * Mathf.Lerp(MinKickForceMult, MaxKickForceMult, chargeRatio);

        float finalElevation = Mathf.Lerp(MinKickElevation, MaxKickElevation, chargeRatio);

        ExecuteBallRelease(finalForce, KickFriction, finalElevation);

    }


    public void Pass()

    {

        float passForce = _player.Stats.ShootPower * PassForceMult;

        ShowActionVisual(new Color(0, 1, 0, 0.5f));

        ExecuteBallRelease(passForce, PassFriction, PassElevation);

    }


    private void ExecuteBallRelease(float force, float friction, float elevationAngle)

    {

        if (!_hasBall || _currentBall == null) return;

        var ballMachine = _currentBall.GetNode<BallStateMachine>("StateMachine");

        ballMachine.ChangeState("Free");

        Vector3 forwardDir = -_player.GetNode<Node3D>("Rotator").GlobalTransform.Basis.Z;

        Vector3 upDir = Vector3.Up * elevationAngle;

        Vector3 finalDirection = (forwardDir + upDir).Normalized();

        _currentBall.LinearDamp = friction;

        _currentBall.ApplyCentralImpulse(finalDirection * force);

        _hasBall = false; _currentBall = null;

        _player.StateMachine.ChangeState("Move");

    }


    private async void ShowActionVisual(Color color)

    {

        var mesh = _player.GetNodeOrNull<MeshInstance3D>("Visuals/MeshInstance3D");

        if (mesh == null) return;

        mesh.Transparency = 0.5f;

        await ToSignal(GetTree().CreateTimer(0.15f), "timeout");

        mesh.Transparency = 0.0f;

    }


    public bool HasBall() => _hasBall;

} 
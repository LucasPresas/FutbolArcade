using Godot;
using System;

public partial class PlayerBase : CharacterBody3D
{
    [ExportGroup("Componentes Core")]
    [Export] public PlayerStats Stats;
    [Export] public BallHandler BallHandler;
    [Export] public PlayerStateMachine StateMachine;

    [ExportGroup("Configuración de Control")]
    [Export] public bool IsUserControlled = false;
    [Export] public Node HumanControllerNode;
    [Export] public Node AiControllerNode;

    public IController Controller { get; private set; }

    [ExportGroup("Identidad y Objetivos")]
    [Export] public string TeamName = "Local";
    [Export] public Goal MyGoal;
    [Export] public Goal TargetGoal;

    [ExportGroup("UI del Jugador")]
    [Export] public Label ChargeLabel;

    // Reinicio de posición
    private Vector3 _initialPosition;
    private Vector3 _initialRotation;

    // Estado de freeze
    private bool _isFrozen = false;
    public bool IsFrozen => _isFrozen;

    // Parámetros tackle y corrección
    private const float TACKLE_VERTICAL_CLAMP = 0.6f;
    private const float TACKLE_MIN_PUSH = 1.5f;
    private const float TACKLE_MAX_PUSH = 6.0f;
    private const float AIR_CORRECT_VELOCITY = -1.2f;

    public override void _Ready()
    {
        _initialPosition = GlobalPosition;
        _initialRotation = GetNode<Node3D>("Rotator").Rotation;

        AddToGroup("Players");
        AddToGroup(TeamName);

        if (Stats == null || BallHandler == null || StateMachine == null)
        {
            GD.PrintErr($"[PlayerBase] {Name}: Faltan componentes en el Inspector.");
            return;
        }

        SetupUI();
        SetupController();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Corrección física periódica para evitar flotado
        CorrectFloating();

        // Si tenés lógica propia de movimiento, respetá IsFrozen:
        // if (IsFrozen) return;
    }

    private void SetupUI()
    {
        if (ChargeLabel != null)
        {
            ChargeLabel.Visible = false;
            ChargeLabel.Text = "0%";
        }
    }

    private void SetupController()
    {
        if (IsUserControlled)
        {
            Controller = HumanControllerNode as IController;
            ToggleNode(AiControllerNode, false);
            ToggleNode(HumanControllerNode, true);
        }
        else
        {
            Controller = AiControllerNode as IController;
            ToggleNode(HumanControllerNode, false);
            ToggleNode(AiControllerNode, true);
        }

        if (Controller == null)
            GD.PrintErr($"[PlayerBase] {Name}: El controlador asignado no implementa IController.");
    }

    public void ResetToInitialPosition()
    {
        GlobalPosition = _initialPosition;
        Velocity = Vector3.Zero;
        GetNode<Node3D>("Rotator").Rotation = _initialRotation;

        StateMachine.ChangeState("idle");
        GD.Print($"[PlayerBase] {Name} ({TeamName}) reseteado a posición inicial.");
    }

    private void ToggleNode(Node node, bool active)
    {
        if (node == null) return;
        node.SetProcess(active);
        node.SetPhysicsProcess(active);
    }

    public bool IsTeammate(PlayerBase other) => other != null && other.TeamName == TeamName;

    // ---------------------------
    // Freeze
    // ---------------------------
    public void SetFrozen(bool frozen)
    {
        if (_isFrozen == frozen)
        {
            GD.Print($"[PlayerBase] {Name} SetFrozen llamado con el mismo estado ({frozen}).");
            return;
        }

        _isFrozen = frozen;
        GD.Print($"[PlayerBase] {Name} SetFrozen -> {(frozen ? "CONGELADO" : "DESCONGELADO")}");

        SetProcess(!frozen);
        SetPhysicsProcess(!frozen);

        // Limpiar velocidad para evitar movimiento residual
        Velocity = Vector3.Zero;

        if (HumanControllerNode != null)
        {
            HumanControllerNode.SetProcess(!frozen);
            HumanControllerNode.SetPhysicsProcess(!frozen);
            GD.Print($"[PlayerBase] {Name} HumanControllerNode process set to {!frozen}");
        }

        if (AiControllerNode != null)
        {
            AiControllerNode.SetProcess(!frozen);
            AiControllerNode.SetPhysicsProcess(!frozen);
            GD.Print($"[PlayerBase] {Name} AiControllerNode process set to {!frozen}");
        }

        if (StateMachine != null)
        {
            StateMachine.SetProcess(!frozen);
            StateMachine.SetPhysicsProcess(!frozen);
            GD.Print($"[PlayerBase] {Name} StateMachine process set to {!frozen}");
        }

        var anim = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (anim != null)
        {
            if (frozen) { anim.Stop(); GD.Print($"[PlayerBase] {Name} AnimationPlayer stopped"); }
            else { anim.PlaybackActive = true; GD.Print($"[PlayerBase] {Name} AnimationPlayer resumed"); }
        }

        var nav = GetNodeOrNull<NavigationAgent3D>("NavigationAgent3D");
        if (nav != null)
        {
            nav.SetProcess(!frozen);
            nav.SetPhysicsProcess(!frozen);
            if (frozen) nav.TargetPosition = GlobalPosition;
            GD.Print($"[PlayerBase] {Name} NavigationAgent3D process set to {!frozen}");
        }

        if (Controller is Node controllerNode)
        {
            controllerNode.SetProcess(!frozen);
            controllerNode.SetPhysicsProcess(!frozen);
            GD.Print($"[PlayerBase] {Name} Controller node process set to {!frozen}");
        }
    }

    // ---------------------------
    // Tackle
    // ---------------------------
    // Intenta tacklear a un rival cercano; devuelve true si se aplicó
    public bool AttemptTackle(float maxDistance = 2.0f, float pushStrength = 4.0f)
    {
        if (_isFrozen)
        {
            GD.Print($"[PlayerBase] {Name} AttemptTackle ignorado: está congelado.");
            return false;
        }

        PlayerBase best = null;
        float bestDist = float.MaxValue;
        foreach (Node n in GetTree().GetNodesInGroup("Players"))
        {
            if (n is PlayerBase p && p != this)
            {
                float d = GlobalPosition.DistanceTo(p.GlobalPosition);
                if (d < bestDist && d <= maxDistance)
                {
                    bestDist = d;
                    best = p;
                }
            }
        }

        if (best == null)
        {
            GD.Print($"[PlayerBase] {Name} AttemptTackle: no hay objetivo en rango ({maxDistance}).");
            return false;
        }

        Vector3 dir = (best.GlobalPosition - GlobalPosition);
        dir = new Vector3(dir.X, 0, dir.Z).Normalized();
        Vector3 push = dir * pushStrength;
        push.Y = 0.28f;

        GD.Print($"[TACKLE] {Name} tacklea a {best.Name} con push {push}");

        best.ApplyTackle(push);

        // Intento forzar que suelte la pelota si tiene (llamada directa si existe)
        if (best.BallHandler != null)
        {
            try
            {
                if (best.BallHandler.HasBall())
                {
                    best.BallHandler.Drop();
                    GD.Print($"[TACKLE] {best.Name} forzado a soltar la pelota.");
                }
            }
            catch
            {
                // Si BallHandler no tiene esos métodos exactos, ignoramos (no romper)
            }
        }

        // Retroceso leve al atacante
        Velocity += -dir * 0.8f;

        return true;
    }

    // Aplica un empujón controlado al jugador (usado por el que tacklea).
    public void ApplyTackle(Vector3 push)
    {
        if (_isFrozen)
        {
            GD.Print($"[PlayerBase] {Name} ApplyTackle ignorado porque está congelado.");
            return;
        }

        push = new Vector3(push.X, Mathf.Clamp(push.Y, -Mathf.Abs(TACKLE_VERTICAL_CLAMP), TACKLE_VERTICAL_CLAMP), push.Z);

        float mag = push.Length();
        if (mag < TACKLE_MIN_PUSH)
            push = push.Normalized() * TACKLE_MIN_PUSH;
        else if (mag > TACKLE_MAX_PUSH)
            push = push.Normalized() * TACKLE_MAX_PUSH;

        // Añadimos al velocity actual, preservando la física vertical si ya existe empuje
        Velocity = new Vector3(Velocity.X + push.X, Velocity.Y + push.Y, Velocity.Z + push.Z);

        GD.Print($"[PlayerBase] {Name} ApplyTackle -> push {push} new Velocity {Velocity}");
    }

    // ---------------------------
    // Floating correction
    // ---------------------------
    public void CorrectFloating()
    {
        if (!IsOnFloor())
        {
            if (Velocity.Y > 0.6f)
            {
                Velocity = new Vector3(Velocity.X, Mathf.Min(Velocity.Y, TACKLE_VERTICAL_CLAMP), Velocity.Z);
                GD.Print($"[PlayerBase] {Name} CorrectFloating: clamp Y -> {Velocity.Y}");
            }

            if (Mathf.Abs(Velocity.Y) < 0.05f)
            {
                Velocity = new Vector3(Velocity.X, AIR_CORRECT_VELOCITY, Velocity.Z);
                GD.Print($"[PlayerBase] {Name} CorrectFloating: apply small down velocity {AIR_CORRECT_VELOCITY}");
            }
        }
    }

    // ---------------------------
    // Reset state machine
    // ---------------------------
    public void ResetStateToInitial()
    {
        if (StateMachine != null)
        {
            if (StateMachine.InitialState != null)
            {
                StateMachine.ChangeState(StateMachine.InitialState.Name);
                GD.Print($"[PlayerBase] {Name} StateMachine cambiado a InitialState: {StateMachine.InitialState.Name}");
            }
            else
            {
                StateMachine.ChangeState("idle");
                GD.Print($"[PlayerBase] {Name} StateMachine cambiado a 'idle'");
            }
        }
    }
}

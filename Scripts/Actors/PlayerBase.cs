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

    // Variables para el reinicio de posición
    private Vector3 _initialPosition;
    private Vector3 _initialRotation;

    public override void _Ready()
    {
        // Guardamos la posición inicial para los reinicios de partido
        _initialPosition = GlobalPosition;
        _initialRotation = GetNode<Node3D>("Rotator").Rotation;

        // Añadimos al grupo Players para que el MatchManager nos encuentre
        AddToGroup("Players");

        if (Stats == null || BallHandler == null || StateMachine == null)
        {
            GD.PrintErr($"[PlayerBase] {Name}: Faltan componentes básicos en el Inspector.");
            return;
        }

        SetupUI();
        SetupController();
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
        {
            GD.PrintErr($"[PlayerBase] {Name}: ERROR - El controlador asignado no implementa IController.");
        }
    }

    // --- SISTEMA DE REINICIO ---
    public void ResetToInitialPosition()
    {
        // 1. Teletransportar a la posición inicial
        GlobalPosition = _initialPosition;
        Velocity = Vector3.Zero;
        
        // 2. Resetear rotación del Rotator
        GetNode<Node3D>("Rotator").Rotation = _initialRotation;

        // 3. Volver al estado Idle para frenar cualquier lógica de carrera
        StateMachine.ChangeState("idle");

        GD.Print($"[PlayerBase] {Name} reseteado a posición inicial.");
    }

    private void ToggleNode(Node node, bool active)
    {
        if (node == null) return;
        node.SetProcess(active);
        node.SetPhysicsProcess(active);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Movimiento manejado por la State Machine
    }
}
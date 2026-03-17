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
    // Usamos el nombre del equipo para lógica de pases y defensa
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
        _initialPosition = GlobalPosition;
        _initialRotation = GetNode<Node3D>("Rotator").Rotation;

        // CRUCIAL: Añadimos al jugador al grupo de su equipo específico
        // Esto permite que la IA busque aliados rápidamente: GetTree().GetNodesInGroup("Local")
        AddToGroup("Players");
        AddToGroup(TeamName);

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
        // Activamos solo el nodo que corresponde
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

    public void ResetToInitialPosition()
    {
        GlobalPosition = _initialPosition;
        Velocity = Vector3.Zero;
        
        GetNode<Node3D>("Rotator").Rotation = _initialRotation;

        // Aseguramos que el estado pase a idle (case-insensitive según tu SM)
        StateMachine.ChangeState("idle");

        GD.Print($"[PlayerBase] {Name} ({TeamName}) reseteado.");
    }

    private void ToggleNode(Node node, bool active)
    {
        if (node == null) return;
        node.SetProcess(active);
        node.SetPhysicsProcess(active);
        // Si el nodo tiene lógica visual o de colisión propia, podrías ocultarlo también aquí
    }

    // Método de ayuda para la IA: ¿Este jugador es mi aliado?
    public bool IsTeammate(PlayerBase other)
    {
        return other != null && other.TeamName == this.TeamName;
    }
}
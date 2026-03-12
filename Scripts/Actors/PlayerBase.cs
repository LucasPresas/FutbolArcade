using Godot;
using System;

public partial class PlayerBase : CharacterBody3D
{
    [ExportGroup("Componentes")]
    // Al usar Export, arrastramos los nodos desde el Inspector
    [Export] public PlayerStats Stats; 
    [Export] public BallHandler BallHandler;
    [Export] public PlayerStateMachine StateMachine;

    [ExportGroup("Configuración de Control")]
    [Export] public bool IsUserControlled = false;

    [ExportGroup("Identidad y Objetivos")]
    [Export] public string TeamName = "Local";
    [Export] public Goal MyGoal;
    [Export] public Goal TargetGoal;

    [ExportGroup("UI del Jugador")]
    // Referencia al Label de texto visual
    [Export] public Label ChargeLabel; 

    public IController Controller;

    public override void _Ready()
    {
        // Verificamos que arrastraste todo en el Inspector para evitar NullReference
        if (Stats == null || BallHandler == null || StateMachine == null)
        {
            GD.PrintErr($"[PlayerBase] {Name}: ERROR. Falta asignar componentes en el Inspector.");
            return;
        }

        // Nos aseguramos de que el texto de carga arranque invisible y en 0%
        if (ChargeLabel != null)
        {
            ChargeLabel.Visible = false;
            ChargeLabel.Text = "0%";
        }

        // --- SELECTOR DE CEREBRO ---
        if (IsUserControlled)
        {
            Controller = GetNodeOrNull<IController>("Controllers/HumanInput");
            // Desactivamos el procesamiento de la IA para que no gaste recursos
            var ai = GetNodeOrNull<Node>("Controllers/AiController");
            if (ai != null) ai.SetProcess(false);
        }
        else
        {
            Controller = GetNodeOrNull<IController>("Controllers/AiController");
            // Desactivamos el procesamiento del Input Humano
            var human = GetNodeOrNull<Node>("Controllers/HumanInput");
            if (human != null) human.SetProcess(false);
        }

        if (Controller == null)
        {
            GD.PrintErr($"[PlayerBase] {Name}: No se encontró el controlador en 'Controllers/'.");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // El movimiento real sucede en los estados (MoveState, IdleState, DribbleState)
    }
}
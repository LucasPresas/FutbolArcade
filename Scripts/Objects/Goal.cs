using Godot;
using System;

[GlobalClass] // <--- CRÍTICO: Esto permite que PlayerBase lo reconozca como tipo "Goal"
public partial class Goal : Area3D
{
    [Export] public string GoalType = "Local"; // "Local" o "Visitante"

    public override void _Ready()
    {
        // Conectamos la señal de que algo entró al área
        BodyEntered += OnBallEntered;
    }

    private void OnBallEntered(Node3D body)
    {
        if (body is Ball ball)
        {
            // Si la pelota entra en el ArcoLocal, es gol del Visitante
            string scorer = (GoalType == "Local") ? "Visitante" : "Local";
            
            GD.Print("----------------------------------");
            GD.Print($"¡¡¡ GOL DEL EQUIPO {scorer.ToUpper()} !!!");
            GD.Print($"Detectado en: {GetParent().Name}");
            GD.Print("----------------------------------");

            // Opcional: Resetear pelota al centro
            ball.LinearVelocity = Vector3.Zero;
            ball.AngularVelocity = Vector3.Zero;
            ball.GlobalPosition = Vector3.Zero;
        }
    }
}
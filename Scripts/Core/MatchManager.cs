using Godot;
using System;

public partial class MatchManager : Node
{
    // Esta variable DEBE aparecer en el Inspector después de hacer Build
    [Export] public float ResetDelay = 3.0f;

    private Ball _ball;

    public override void _Ready()
    {
        // Buscamos la pelota
        _ball = GetTree().CurrentScene.FindChild("Ball") as Ball;
        
        // Conectamos la señal del gol
        Goal.OnGoalScored += HandleGoal;
        
        GD.Print("MatchManager listo y esperando goles...");
    }

    private void HandleGoal(string teamName)
    {
        GD.Print($"¡GOL DE {teamName}! Reseteando en {ResetDelay} segundos...");
        
        // Pausamos jugadores
        SetPlayersActive(false);

        // Timer para resetear
        GetTree().CreateTimer(ResetDelay).Timeout += ResetMatch;
    }

    private void ResetMatch()
    {
        // Reset Pelota
        if (_ball != null)
        {
            _ball.GlobalPosition = new Vector3(0, 2, 0);
            _ball.LinearVelocity = Vector3.Zero;
            _ball.AngularVelocity = Vector3.Zero;
        }

        // Reset Jugadores
        var players = GetTree().GetNodesInGroup("Players");
        foreach (Node node in players)
        {
            if (node is PlayerBase player)
            {
                player.ResetToInitialPosition();
            }
        }

        SetPlayersActive(true);
        GD.Print("¡Partido reanudado!");
    }

    private void SetPlayersActive(bool active)
    {
        foreach (Node node in GetTree().GetNodesInGroup("Players"))
        {
            node.SetProcess(active);
            node.SetPhysicsProcess(active);
        }
    }
}
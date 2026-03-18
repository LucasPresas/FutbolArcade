using Godot;
using System;

public partial class MatchManager : Node
{
    [Export] public float ResetDelay = 2.0f; // duración de la celebración (segundos)
    private Ball _ball;

    public override void _Ready()
    {
        _ball = GetTree().CurrentScene.FindChild("Ball") as Ball;
        Goal.OnGoalScored += HandleGoal;
    }

    public override void _ExitTree()
    {
        Goal.OnGoalScored -= HandleGoal;
    }

    // Ahora async: congela todo, espera ResetDelay, resetea y descongela
    private async void HandleGoal(string teamName)
    {
        if (!IsInstanceValid(this) || !IsInsideTree()) return;

        // Congelar pelota y jugadores
        FreezeAll(true);

        // Esperar la duración de celebración
        await ToSignal(GetTree().CreateTimer(ResetDelay), "timeout");

        // Reiniciar posiciones y estados
        if (_ball != null)
        {
            _ball.GlobalPosition = new Vector3(0, 2, 0);
            _ball.ResetPhysics();
        }

        foreach (Node node in GetTree().GetNodesInGroup("Players"))
        {
            if (node is PlayerBase player) player.ResetToInitialPosition();
        }

        // Descongelar
        FreezeAll(false);
    }

    private void FreezeAll(bool freeze)
    {
        // Congela/descongela pelota
        if (_ball != null) _ball.SetFrozen(freeze);

        // Congela/descongela jugadores
        foreach (Node node in GetTree().GetNodesInGroup("Players"))
        {
            if (node is PlayerBase player)
            {
                player.SetFrozen(freeze);
            }
            else
            {
                // Si hay nodos que no son PlayerBase pero están en el grupo, intentamos togglear
                node.SetProcess(!freeze);
                node.SetPhysicsProcess(!freeze);
            }
        }
    }

    // Mantengo este método por compatibilidad si lo usás en otro lado
    private void SetPlayersActive(bool active)
    {
        if (!IsInstanceValid(this) || !IsInsideTree()) return;
        foreach (Node node in GetTree().GetNodesInGroup("Players"))
        {
            node.SetProcess(active);
            node.SetPhysicsProcess(active);
        }
    }
}

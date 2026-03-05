using Godot;
using System;

namespace AcmeStriker;

public partial class PlayerController : CharacterBase
{
    [Export] public Area3D KickPicker;

    public override void _PhysicsProcess(double delta)
    {
        // --- 1. Lógica del Jugador (Teclas) ---
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        Vector3 direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();
        
        MoveCharacter(direction);

        if (Input.IsActionJustPressed("action_shoot"))
        {
            TryAction(StatsResource.ShootForce, new Vector3(0, 0.1f, 0));
        }
        else if (Input.IsActionJustPressed("action_pass"))
        {
            TryAction(StatsResource.PassForce, Vector3.Zero);
        }

        // --- 2. IMPORTANTE: Llamar a la base ---
        // Esto ejecuta el MoveAndSlide() y la gravedad que están en CharacterBase
        base._PhysicsProcess(delta);
    }

    private void TryAction(float force, Vector3 arc)
    {
        if (KickPicker == null || StatsResource == null) return;

        var bodies = KickPicker.GetOverlappingBodies();
        foreach (var body in bodies)
        {
            if (body is Ball ball) 
            {
                Vector3 lookDir = -Visuals.GlobalTransform.Basis.Z;
                Vector3 finalDir = (lookDir + arc).Normalized();
                ball.ReceiveKick(finalDir, force);
                break;
            }
        }
    }
}
using Godot;
using System;

namespace AcmeStriker;

public partial class PlayerController : CharacterBase
{
    public override void _PhysicsProcess(double delta)
    {
        // 1. Obtenemos el vector de movimiento usando nuestras nuevas acciones
        // Esto devuelve un Vector2 entre -1 y 1
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        
        // 2. Lo pasamos a 3D (X = Horizontal, Z = Vertical en el suelo)
        Vector3 direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();

        // 3. Llamamos a la función de la BASE que tú programaste
        if (direction.Length() > 0)
        {
            MoveCharacter(direction);
        }
        else
        {
            // Si no tocamos nada, pasamos un vector cero para que frene
            MoveCharacter(Vector3.Zero);
        }
    }
}
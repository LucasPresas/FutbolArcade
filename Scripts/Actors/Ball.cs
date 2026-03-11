using Godot;
using System;

public partial class Ball : RigidBody3D
{
    public override void _Ready()
    {
        // Conectamos la señal de colisión a nuestro método
        BodyEntered += OnBallCollision;
    }

    private void OnBallCollision(Node body)
    {
        // Pasamos el nombre a minúsculas para evitar errores de tipeo
        string name = body.Name.ToString().ToLower();

        // Verificamos si el nombre contiene las palabras clave
        if (name.Contains("poste") || name.Contains("crossbar") || name.Contains("fisica"))
        {
            // Buscamos el nombre del padre (ArcoLocal o ArcoVisitante)
            string side = "Desconocido";
            if (body.GetParent() != null)
            {
                side = body.GetParent().Name;
            }

            // Identificamos si es palo o travesaño por el nombre del nodo
            string part = name.Contains("crossbar") ? "TRAVESSAÑO" : "PALO";

            GD.Print($"----------------------------------");
            GD.Print($"¡¡ CLANK !! [{side}] -> Pegó en el {part}");
            GD.Print($"----------------------------------");
            
            // Aquí podrías agregar un efecto visual o sonido metálico
        }
    }
}
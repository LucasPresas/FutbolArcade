using Godot;

public partial class Ball : RigidBody3D
{
    public override void _Ready()
    {
        // Conectamos la señal de colisión a nuestro método
        BodyEntered += OnBallCollision;
    }

    private void OnBallCollision(Node body)
    {
        if (body == null) return;

        // Convertimos a string explícitamente y normalizamos
        string name = body.Name.ToString().ToLowerInvariant();

        // Verificamos si el nombre contiene las palabras clave
        if (name.Contains("poste") || name.Contains("crossbar") || name.Contains("fisica"))
        {
            // Buscamos el nombre del padre (ArcoLocal o ArcoVisitante)
            string side = body.GetParent() != null ? body.GetParent().Name : "Desconocido";

            // Identificamos si es palo o travesaño por el nombre del nodo
            string part = name.Contains("crossbar") ? "TRAVESSAÑO" : "PALO";

            GD.Print("----------------------------------");
            GD.Print($"¡¡ CLANK !! [{side}] -> Pegó en el {part}");
            GD.Print("----------------------------------");
            
            // Aquí podrías agregar un efecto visual o sonido metálico
        }
    }

    /// <summary>
    /// Congela o descongela la pelota (detiene física y procesamiento).
    /// </summary>
    public void SetFrozen(bool frozen)
    {
        SetProcess(!frozen);
        SetPhysicsProcess(!frozen);

        if (frozen)
        {
            LinearVelocity = Vector3.Zero;
            AngularVelocity = Vector3.Zero;
            Sleeping = true;
        }
        else
        {
            Sleeping = false;
        }
    }

    /// <summary>
    /// Limpia velocidades y asegura que la física esté lista.
    /// </summary>
    public void ResetPhysics()
    {
        LinearVelocity = Vector3.Zero;
        AngularVelocity = Vector3.Zero;
        Sleeping = false;
    }
}

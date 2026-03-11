using Godot;
using System;

public partial class MovementComponent : Node
{
    private PlayerBase _player;

    public override void _Ready()
    {
        // Buscamos al PlayerBase subiendo en la jerarquía
        _player = GetParent().GetParentOrNull<PlayerBase>();
        
        if (_player == null)
        {
            GD.PrintErr("[MovementComponent] ERROR: No se encontró PlayerBase en la raíz.");
        }
        else
        {
            GD.Print("[MovementComponent] Iniciado y conectado a PlayerBase.");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Validaciones de seguridad
        if (_player == null || _player.Controller == null || _player.Stats == null) return;

        Vector3 direction = _player.Controller.GetMoveDirection();
        Vector3 velocity = _player.Velocity;

        // 1. Aplicar Gravedad
        if (!_player.IsOnFloor())
        {
            velocity.Y -= 9.8f * (float)delta;
        }

        // 2. Lógica de Movimiento y Rotación
        if (direction != Vector3.Zero)
        {
            // Movimiento horizontal
            velocity.X = direction.X * _player.Stats.Speed;
            velocity.Z = direction.Z * _player.Stats.Speed;
            
            // --- ROTACIÓN DEL NODO ROTATOR ---
            Node3D rotator = _player.GetNodeOrNull<Node3D>("Rotator");
            if (rotator != null)
            {
                // Calculamos el ángulo hacia donde nos movemos.
                // Usamos Atan2(X, Z) para obtener la orientación en el plano del suelo.
                float targetAngle = Mathf.Atan2(-direction.X, -direction.Z);
                
                // Aplicamos LerpAngle para que el giro no sea instantáneo (se ve más profesional)
                float rotationSpeed = _player.Stats.RotationSpeed;
                float currentRotationY = rotator.Rotation.Y;
                
                rotator.Rotation = new Vector3(
                    0,
                    (float)Mathf.LerpAngle(currentRotationY, targetAngle, rotationSpeed * (float)delta),
                    0
                );
            }
        }
        else
        {
            // Frenado progresivo (Fricción)
            velocity.X = Mathf.MoveToward(velocity.X, 0, _player.Stats.Acceleration * (float)delta);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, _player.Stats.Acceleration * (float)delta);
        }

        // 3. Aplicar velocidad y ejecutar físicas
        _player.Velocity = velocity;
        _player.MoveAndSlide();
    }
}
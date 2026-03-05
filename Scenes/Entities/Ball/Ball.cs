using Godot;
using System;

namespace AcmeStriker.Entities;

public partial class Ball : RigidBody3D
{
    [Export] public float Decceleration = 0.95f;

    public override void _Ready()
    {
        // Aseguramos que la pelota pueda rodar
        AxisLockAngularX = false;
        AxisLockAngularZ = false;
    }

    // Esta función la llamará el Player cuando patee
    public void ApplyKick(Vector3 direction, float force)
    {
        // Aplicamos un impulso instantáneo al centro de la pelota
        ApplyCentralImpulse(direction * force);
    }
}
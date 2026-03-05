using Godot;
using System;

namespace AcmeStriker;

public partial class Ball : RigidBody3D
{
    [Export] public float AirResistance = 0.5f; // Resistencia al aire extra

    public override void _Ready()
    {
        // Configuraciones iniciales de física por código para asegurar el comportamiento
        ContactMonitor = true;
        MaxContactsReported = 5;
        
        // Permite que ruede libremente
        AxisLockAngularX = false;
        AxisLockAngularY = false;
        AxisLockAngularZ = false;
    }

    /// <summary>
    /// Método llamado por el Player o la IA para patear la pelota.
    /// </summary>
    public void ReceiveKick(Vector3 direction, float force)
    {
        // 1. Resetear fuerzas actuales para que el tiro no salga desviado por la inercia previa
        LinearVelocity = Vector3.Zero;
        AngularVelocity = Vector3.Zero;

        // 2. Aplicar el nuevo impulso
        // Usamos ApplyCentralImpulse para que la fuerza afecte al centro de masa
        ApplyCentralImpulse(direction * force);
        
        GD.Print($"Pelota pateada hacia: {direction} con fuerza: {force}");
    }

    public override void _PhysicsProcess(double delta)
    {
        // Opcional: Podríamos añadir un multiplicador de fricción aquí si 
        // queremos que la pelota se detenga más rápido de lo que permite el motor.
    }
}
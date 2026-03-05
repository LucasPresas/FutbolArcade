using Godot;
using System;

namespace AcmeStriker;

public partial class CharacterBase : CharacterBody3D
{
    [ExportGroup("Base Settings")]
    [Export] public PlayerStats StatsResource;
    [Export] public Node3D Visuals;

    [ExportGroup("Physics Interaction")]
    [Export] public float PushForce = 1.5f;

    // Obtenemos la gravedad del proyecto para que sea realista
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _PhysicsProcess(double delta)
    {
        // 1. Aplicar gravedad si no estamos en el suelo
        if (!IsOnFloor())
        {
            Vector3 v = Velocity;
            v.Y -= gravity * (float)delta;
            Velocity = v;
        }

        // 2. Mover el cuerpo físico
        MoveAndSlide();

        // 3. Empujar objetos
        HandleCollisionPush();
    }

    public void MoveCharacter(Vector3 direction)
    {
        if (StatsResource == null) return;

        Vector3 velocity = Velocity;
        
        // Mantener la velocidad vertical (gravedad) y actualizar X/Z
        velocity.X = direction.X * StatsResource.MoveSpeed;
        velocity.Z = direction.Z * StatsResource.MoveSpeed;

        if (direction.Length() > 0)
        {
            float targetAngle = Mathf.Atan2(direction.X, direction.Z);
            // Lerp de rotación opcional para que sea más suave
            Visuals.Rotation = new Vector3(0, targetAngle, 0);
        }

        Velocity = velocity;
    }

    private void HandleCollisionPush()
    {
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision = GetSlideCollision(i);
            if (collision.GetCollider() is RigidBody3D rb)
            {
                Vector3 pushDir = -collision.GetNormal();
                // Solo empujamos en el plano horizontal para no hundir la pelota
                pushDir.Y = 0; 
                Vector3 impulse = pushDir * Velocity.Length() * PushForce;
                rb.ApplyImpulse(impulse, collision.GetPosition() - rb.GlobalPosition);
            }
        }
    }
}
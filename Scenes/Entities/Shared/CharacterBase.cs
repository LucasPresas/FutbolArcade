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

    // Obtenemos la gravedad del proyecto
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
    {
        // MAGIA AUTOMÁTICA: Si el hueco en el Inspector está vacío, el código lo busca solo.
        if (Visuals == null)
        {
            Visuals = GetNodeOrNull<Node3D>("Visuals");
            
            if (Visuals == null)
            {
                GD.PrintErr("CRÍTICO: No encontré el nodo 'Visuals' en " + Name + ". Asegúrate de que se llame exactamente 'Visuals'.");
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Gravedad
        if (!IsOnFloor())
        {
            Vector3 v = Velocity;
            v.Y -= gravity * (float)delta;
            Velocity = v;
        }

        // Delegamos el MoveAndSlide al MovementComponent por medio del StateMachine.
        // Solo llamamos MoveAndSlide() aquí si NO estamos usando la StateMachine (Legacy Mode).
        if (!HasNode("StateMachine")) 
        {
            MoveAndSlide();
        }
        
        HandleCollisionPush();
    }

    public void MoveCharacter(Vector3 direction)
    {
        // Si no hay Stats, no nos movemos (evita crasheos)
        if (StatsResource == null) return;

        Vector3 velocity = Velocity;
        
        velocity.X = direction.X * StatsResource.MoveSpeed;
        velocity.Z = direction.Z * StatsResource.MoveSpeed;

        if (direction.Length() > 0)
        {
            float targetAngle = Mathf.Atan2(direction.X, direction.Z);
            
            // RED DE SEGURIDAD: Solo rotamos si el nodo Visuals existe
            if (Visuals != null)
            {
                Visuals.Rotation = new Vector3(0, targetAngle, 0);
            }
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
                pushDir.Y = 0; 
                Vector3 impulse = pushDir * Velocity.Length() * PushForce;
                rb.ApplyImpulse(impulse, collision.GetPosition() - rb.GlobalPosition);
            }
        }
    }
}
using Godot;

namespace AcmeStriker;

[GlobalClass]
public partial class PlayerStats : Resource
{
    [Export] public float MoveSpeed = 12.0f;
    [Export] public float KickForce = 15.0f; // Fuerza base
    [Export] public float ShootForce = 25.0f; // <-- ESTA FALTA
    [Export] public float PassForce = 12.0f;  // <-- ESTA TAMBIÉN
}
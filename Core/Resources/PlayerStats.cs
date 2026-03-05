using Godot;

namespace AcmeStriker; // Usamos un namespace global simple

[GlobalClass]
public partial class PlayerStats : Resource
{
    [Export] public float MoveSpeed = 12.0f;
    [Export] public float KickForce = 15.0f;
}
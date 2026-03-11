using Godot;

[GlobalClass] // Esto hace que aparezca en el menú de "Nuevo Recurso"
public partial class PlayerStats : Resource
{
    [Export] public string ClassName = "Default";

    [ExportGroup("Movimiento Arcade")]
    [Export] public float Speed = 5.0f;
    [Export] public float Acceleration = 10.0f;
    [Export] public float RotationSpeed = 10.0f;

    [ExportGroup("Habilidades RPG")]
    [Export] public float ShootPower = 20.0f;
    [Export(PropertyHint.Range, "1,99")] public int Luck = 10;      // Para el Tackle
    [Export(PropertyHint.Range, "1,99")] public int Technique = 10; // Para el Dribble
}
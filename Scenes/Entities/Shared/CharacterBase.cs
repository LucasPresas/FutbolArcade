using Godot;
namespace AcmeStriker;

public partial class CharacterBase : CharacterBody3D {
    [Export] public PlayerStats StatsResource; 
    [Export] public Node3D VisualsNode;

    public void MoveCharacter(Vector3 direction) {
        Velocity = direction * StatsResource.MoveSpeed;
        MoveAndSlide();
    }
}
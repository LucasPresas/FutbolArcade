using Godot;
namespace AcmeStriker;

public partial class PlayerController : CharacterBase {
    public override void _PhysicsProcess(double delta) {
        Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Vector3 dir = new Vector3(input.X, 0, input.Y);
        MoveCharacter(dir);
    }
}
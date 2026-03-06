using Godot;

public interface IController
{
    Vector3 GetMoveDirection();
    bool IsPassing();
    bool IsShooting();
    bool IsTackling();
}

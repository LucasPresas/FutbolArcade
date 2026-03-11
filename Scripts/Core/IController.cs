using Godot;

public interface IController
{
    Vector3 GetMoveDirection();
    bool IsShooting();
    bool IsPassing();
    bool IsTackling();
    bool IsCharging();
}
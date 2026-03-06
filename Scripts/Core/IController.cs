using Godot;

public interface IController
{
    Vector2 GetMoveDirection();
    bool IsPassing();
    bool IsShooting();
    bool IsTackling();
}

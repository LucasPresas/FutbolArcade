using Godot;
using System;

// Debe heredar de State (ya corregido)
public partial class BallFreeState : State
{
    private Ball _ball;

    public override void _Ready()
    {
        _ball = GetOwner<Ball>();
    }

    public override void Enter()
    {
        if (_ball != null) _ball.SetFrozen(false);
        GD.Print("[Ball] Estado: Free");
    }
}

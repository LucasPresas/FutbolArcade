using Godot;

// CAMBIO CRÍTICO: Debe heredar de 'State', no de 'Node' ni de 'BallState'
public partial class BallFreeState : State 
{
    private Ball _ball;

    public override void _Ready()
    {
        _ball = GetOwner<Ball>();
    }

    public override void Enter()
    {
        if (_ball != null) _ball.Freeze = false;
        GD.Print("[Ball] Estado: Free");
    }
}
using Godot;

public partial class BallCarriedState : State 
{
    private Ball _ball;
    private Marker3D _targetPoint;

    public override void _Ready()
    {
        _ball = GetOwner<Ball>();
    }

    public void SetTarget(Marker3D point) 
    {
        _targetPoint = point;
        if (_targetPoint != null)
        {
            GD.Print("[BallCarriedState] Target asignado correctamente.");
        }
    }

    public override void Enter()
    {
        // Importante: Al entrar, reseteamos velocidades para que no "luche" con el Lerp
        if (_ball != null)
        {
            _ball.LinearVelocity = Vector3.Zero;
            _ball.AngularVelocity = Vector3.Zero;
        }
    }

    public override void PhysicsUpdate(float delta)
    {
        // 1. Verificación de seguridad
        if (_ball == null) return;

        if (_targetPoint == null)
        {
            // Solo imprimimos esto si realmente falta el punto
            GD.PrintErr("[BallCarriedState] ERROR: No hay _targetPoint. La pelota no sabe a dónde ir.");
            return;
        }

        // 2. Movimiento suave hacia el pie (DribblePoint)
        // Subí el valor de 30.0f a 40.0f si sentís que la pelota "llega tarde" al correr
        _ball.GlobalPosition = _ball.GlobalPosition.Lerp(_targetPoint.GlobalPosition, 40.0f * delta);
        
        // 3. Mantenemos la pelota "quieta" respecto a la rotación física
        _ball.LinearVelocity = Vector3.Zero;
        _ball.AngularVelocity = Vector3.Zero;
    }
}
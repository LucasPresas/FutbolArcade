using Godot;

public partial class Ball : RigidBody3D // Or RigidBody3D
{
    // The ball will also have a StateMachine to handle Freedom, Carried, Passed, Shot
    [Export] public NodePath StateMachinePath { get; set; }

    public StateMachine StateMachine { get; private set; }

    public override void _Ready()
    {
        AddToGroup("ball");
        ContactMonitor = true;
        MaxContactsReported = 5;

        // Permite que ruede libremente
        AxisLockAngularX = false;
        AxisLockAngularY = false;
        AxisLockAngularZ = false;

        if (StateMachinePath != null)
        {
            StateMachine = GetNode<StateMachine>(StateMachinePath);
        }
    }

    public void ReceiveKick(Vector3 direction, float force)
    {
        // 1. Resetear fuerzas
        LinearVelocity = Vector3.Zero;
        AngularVelocity = Vector3.Zero;

        // 2. Aplicar el nuevo impulso al centro de masa
        ApplyCentralImpulse(direction * force);
        GD.Print($"Pelota pateada hacia: {direction} con fuerza: {force}");
    }
}

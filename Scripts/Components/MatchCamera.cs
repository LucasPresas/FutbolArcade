using Godot;

public partial class MatchCamera : Camera3D
{
    // --- Seguimiento Dinámico ---
    [Export] public float FollowSpeed { get; set; } = 3.0f;
    [Export] public Vector2 DeadzoneSize { get; set; } = new Vector2(4.0f, 3.0f);
    
    // --- Límites de Cancha ---
    [Export] public float MinX { get; set; } = -12f;
    [Export] public float MaxX { get; set; } = 12f;
    [Export] public float MinZ { get; set; } = -8f;
    [Export] public float MaxZ { get; set; } = 8f;

    // --- Control de Zoom (Perspectiva) ---
    [Export] public float DefaultFov { get; set; } = 50f;
    [Export] public float ZoomedFov { get; set; } = 30f;
    [Export] public float ZoomSpeed { get; set; } = 5.0f;

    private Node3D _target;
    private Vector3 _startPosition;
    private float _targetFov;
    private bool _isZoomRequested = false;
    private Node3D _zoomOverrideTarget = null;

    public override void _Ready()
    {
        _startPosition = GlobalPosition;
        _targetFov = DefaultFov;
        Fov = DefaultFov;
    }

    private void EnsureTarget()
    {
        if (_target != null && IsInstanceValid(_target)) return;

        // Intentar encontrar la pelota por nombre o grupo
        _target = GetParent().GetNodeOrNull<Node3D>("Ball");
        if (_target == null)
            _target = GetTree().GetFirstNodeInGroup("ball") as Node3D;
    }

    public override void _Process(double delta)
    {
        EnsureTarget();
        HandleZoomAndTarget(delta);
        HandlePositionLerp(delta);
    }

    private void HandleZoomAndTarget(double delta)
    {
        if (_isZoomRequested && _zoomOverrideTarget != null && IsInstanceValid(_zoomOverrideTarget))
        {
            // Zoom in on the specific player charging the shot/pass
            _targetFov = ZoomedFov;
        }
        else
        {
            // Normal play: Follow Ball
            _targetFov = DefaultFov;
        }

        // Lerp FOV
        Fov = Mathf.Lerp(Fov, _targetFov, ZoomSpeed * (float)delta);
    }

    private void HandlePositionLerp(double delta)
    {
        Node3D currentTarget = (_isZoomRequested && _zoomOverrideTarget != null) ? _zoomOverrideTarget : _target;
        
        if (currentTarget == null) return;

        Vector3 targetPos = currentTarget.GlobalPosition;
        Vector3 camPos = GlobalPosition;

        // Calcula el offset ideal de la cámara manteniendo su elevación e inclinación original
        Vector3 idealCamPos = new Vector3(targetPos.X, _startPosition.Y, targetPos.Z + _startPosition.Z);

        // --- Lógica de Deadzone ---
        float diffX = idealCamPos.X - camPos.X;
        float diffZ = idealCamPos.Z - camPos.Z;

        Vector3 finalTargetPos = camPos;

        if (Mathf.Abs(diffX) > DeadzoneSize.X)
            finalTargetPos.X = idealCamPos.X - Mathf.Sign(diffX) * DeadzoneSize.X;
        
        if (Mathf.Abs(diffZ) > DeadzoneSize.Y)
            finalTargetPos.Z = idealCamPos.Z - Mathf.Sign(diffZ) * DeadzoneSize.Y;

        // --- Lerp + Clamp ---
        Vector3 newPos = camPos.Lerp(finalTargetPos, FollowSpeed * (float)delta);
        newPos.X = Mathf.Clamp(newPos.X, MinX, MaxX);
        newPos.Z = Mathf.Clamp(newPos.Z, MinZ, MaxZ);

        GlobalPosition = newPos;
    }

    // --- API for Player.cs to request zoom ---
    
    public void RequestZoomOn(Node3D player)
    {
        _isZoomRequested = true;
        _zoomOverrideTarget = player;
    }

    public void CancelZoom()
    {
        _isZoomRequested = false;
        _zoomOverrideTarget = null;
    }
}

using Godot;

public partial class AIController : Node, IController
{
    private const float ShootDistance = 12f;
    private const float PassDistanceMin = 5f;
    private const float PassDistanceMax = 15f;
    
    // Set to "goal_0" for Team 0 AI (companions), "goal_1" for Team 1 AI (enemies)
    [Export] public string TargetGoalGroup { get; set; } = "goal_1";
    
    // AI Role definition
    public enum AIRole { Defender, Midfielder, Attacker }
    [Export] public AIRole Role { get; set; } = AIRole.Midfielder;

    private Ball _ball;
    private Vector3 _targetGoalPosition;
    private Vector3 _ownGoalPosition;
    private BallHandlerComponent _ballHandler;
    private ActorBase _actor;
    private Vector3 _startPosition;

    private bool _referencesReady = false;

    // AI States
    private enum AIState { Positioning, Chasing, Attacking, Defending }
    private AIState _currentState = AIState.Positioning;

    public override void _Ready()
    {
        _actor = GetParent().GetParent<ActorBase>();
        _ballHandler = _actor?.GetNodeOrNull<BallHandlerComponent>("Components/BallHandler");
        
        // Wait 1 frame to get valid global position after scene tree is built
        CallDeferred(nameof(StoreStartPosition));
    }

    private void StoreStartPosition()
    {
        if (_actor != null)
            _startPosition = _actor.GlobalPosition;
    }

    private void EnsureReferences()
    {
        if (_referencesReady) return;

        _ball ??= GetTree().GetFirstNodeInGroup("ball") as Ball;

        if (_targetGoalPosition == Vector3.Zero)
        {
            var targetGoal = GetTree().GetFirstNodeInGroup(TargetGoalGroup) as Node3D;
            if (targetGoal != null)
                _targetGoalPosition = new Vector3(targetGoal.GlobalPosition.X, 0, targetGoal.GlobalPosition.Z);
            
            // Infer own goal by opposing group logic (quick hack for prototype)
            string ownGoalStr = TargetGoalGroup == "goal_1" ? "goal_0" : "goal_1";
            var ownGoal = GetTree().GetFirstNodeInGroup(ownGoalStr) as Node3D;
            if (ownGoal != null)
                _ownGoalPosition = new Vector3(ownGoal.GlobalPosition.X, 0, ownGoal.GlobalPosition.Z);
        }

        if (_ball != null && _targetGoalPosition != Vector3.Zero && _startPosition != Vector3.Zero)
            _referencesReady = true;
    }

    public Vector3 GetMoveDirection()
    {
        EnsureReferences();
        if (_ball == null || _actor == null || !_referencesReady) return Vector3.Zero;

        UpdateAIState();

        Vector3 targetPos = _actor.GlobalPosition;

        switch (_currentState)
        {
            case AIState.Chasing:
                targetPos = _ball.GlobalPosition;
                break;
                
            case AIState.Attacking:
                targetPos = _targetGoalPosition;
                break;
                
            case AIState.Positioning:
                // Return to home position, sliding on Z slightly with the ball to mark the zone
                targetPos = _startPosition;
                targetPos.Z = Mathf.Lerp(_startPosition.Z, _ball.GlobalPosition.Z, 0.4f);
                break;
                
            case AIState.Defending:
                // Stand between the ball and our goal, staying within our assigned lane (X)
                Vector3 idealDefendingPos = _ownGoalPosition.Lerp(_ball.GlobalPosition, 0.3f);
                targetPos = new Vector3(_startPosition.X, 0, idealDefendingPos.Z);
                break;
        }

        Vector3 dir = targetPos - _actor.GlobalPosition;
        dir.Y = 0;

        // If very close to target position (except when chasing ball), stop moving
        if (_currentState != AIState.Chasing && _currentState != AIState.Attacking && dir.LengthSquared() < 1.5f)
        {
            return Vector3.Zero;
        }

        return dir.Normalized();
    }

    private void UpdateAIState()
    {
        if (_ballHandler?.CarriedBall != null)
        {
            _currentState = AIState.Attacking;
            return;
        }

        float distToBall = _actor.GlobalPosition.DistanceTo(_ball.GlobalPosition);
        
        // Find if someone else on my team is closer to the ball
        bool amIClosest = true;
        foreach (var node in GetTree().GetNodesInGroup("all_players"))
        {
            if (node is ActorBase otherActor && otherActor.TeamId == _actor.TeamId && otherActor != _actor)
            {
                float otherDist = otherActor.GlobalPosition.DistanceTo(_ball.GlobalPosition);
                
                // Hysteresis + ID tiebreaker to prevent flickering and clustering when side-by-side
                if (otherDist < distToBall - 0.5f)
                {
                    amIClosest = false;
                    break;
                }
                else if (Mathf.Abs(otherDist - distToBall) <= 0.5f && otherActor.GetInstanceId() < _actor.GetInstanceId())
                {
                    amIClosest = false;
                    break;
                }
            }
        }

        // Is enemy carrying the ball?
        bool enemyHasBall = false;
        var carrier = _ball.GetParent()?.GetParent() as ActorBase; // Ball -> BallHandler -> Components -> ActorBase
        if (carrier != null && carrier.TeamId != _actor.TeamId)
        {
            enemyHasBall = true;
        }

        if (enemyHasBall)
        {
            _currentState = amIClosest ? AIState.Chasing : AIState.Defending;
        }
        else
        {
            // Ball is free
            if (amIClosest)
            {
                _currentState = AIState.Chasing;
            }
            else
            {
                // Ball is free but I'm not closest, or teammate has ball
                _currentState = AIState.Positioning;
            }
        }
    }

    public bool IsShooting()
    {
        EnsureReferences();
        if (_actor == null || _ballHandler?.CarriedBall == null) return false;

        Vector3 pos = _actor.GlobalPosition;
        float dist = new Vector2(pos.X - _targetGoalPosition.X, pos.Z - _targetGoalPosition.Z).Length();
        return dist < ShootDistance;
    }

    public bool IsPassing()
    {
        EnsureReferences();
        if (_actor == null || _ballHandler?.CarriedBall == null) return false;

        // AI passes if they are far from goal but blocked (simplified: random chance if teammate nearby)
        // Just a prototype hook for now, AI tries to shoot instead of pass most times
        return false; 
    }
    
    public bool IsTackling()
    {
        EnsureReferences();
        if (_actor == null) return false;
        
        // Tackle if close to the enemy carrying the ball
        if (_currentState == AIState.Chasing)
        {
            var carrier = _ball.GetParent()?.GetParent() as ActorBase;
            if (carrier != null && carrier.TeamId != _actor.TeamId)
            {
                // Radius of capsule is 0.5. Two bodies colliding are 1.0m apart minimum.
                // 1.6f gives them a tiny margin where they just touch.
                float distToCarrier = _actor.GlobalPosition.DistanceTo(carrier.GlobalPosition);
                if (distToCarrier <= 1.6f) return true;
            }
        }
        return false;
    }
}

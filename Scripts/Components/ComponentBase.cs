using Godot;

public abstract partial class ComponentBase : Node
{
    protected ActorBase Actor;

    public override void _Ready()
    {
        // Components should be added under a 'Components' Node which is under the ActorBase
        // e.g. ActorBase -> Components -> MovementComponent
        Actor = GetParent().GetParent<ActorBase>(); 
        
        if (Actor == null)
        {
            GD.PrintErr($"Component {Name} could not find ActorBase in its parents.");
        }
    }
}

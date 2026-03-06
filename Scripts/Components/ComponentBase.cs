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
            GD.PrintErr($"CRÍTICO: Component {Name} no pudo encontrar a ActorBase en sus ancestros. Verifica el árbol de nodos.");
        }
        else 
        {
            GD.Print($"[OK] {Name} conectado a {Actor.Name}");
        }
    }
}

using Godot;

public abstract partial class State : Node
{
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(float delta) { }
    public virtual void PhysicsUpdate(float delta) { }

    public new T GetOwner<T>() where T : Node
    {
        Node current = GetParent();
        while (current != null)
        {
            if (current is T target) return target;
            current = current.GetParent();
        }
        return null;
    }
}
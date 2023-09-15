using OpenTK.Mathematics;
using Touhou.Scenes;

namespace Touhou.Objects;
public abstract class Entity {

    public Scene Scene { get; set; }

    public Vector2 Position { get; set; }

    public event Action Destroyed;
    public bool IsDestroyed { get; private set; }

    public bool CanCollide { get; set; } = true;
    //public CollisionType CollisionType { get; protected set; }
    //public List<CollisionGroups> CollisionGroups { get; private set; } = new();
    public List<Hitbox> Hitboxes { get; private set; } = new();

    public virtual void Update() { }
    public virtual void Render() { }
    public virtual void PostRender() { }

    public virtual void DebugRender() { }

    public virtual void Destroy() {
        IsDestroyed = true;
        Destroyed?.Invoke();
    }
}
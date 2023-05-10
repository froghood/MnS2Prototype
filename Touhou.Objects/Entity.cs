using SFML.System;
using Touhou.Scenes;

namespace Touhou.Objects;
public abstract class Entity {

    public Scene Scene { get; set; }

    public Vector2f Position { get; set; }

    public event Action Destroyed;
    public bool IsDestroyed { get; private set; }

    public bool CanCollide { get; set; } = true;
    public CollisionType CollisionType { get; protected set; }
    public List<int> CollisionFilters { get; private set; } = new();
    public List<Hitbox> Hitboxes { get; private set; } = new();

    public abstract void Update(Time time, float delta);
    public abstract void Render(Time time, float delta);
    public abstract void Finalize(Time time, float delta);

    public virtual void DebugRender(Time time, float delta) { }

    public virtual void Destroy() {
        IsDestroyed = true;
        Destroyed?.Invoke();
    }

    public virtual void Collide(Entity entity) { }
}
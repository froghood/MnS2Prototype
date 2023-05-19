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
    public List<int> CollisionGroups { get; private set; } = new();
    public List<Hitbox> Hitboxes { get; private set; } = new();

    public abstract void Update();
    public abstract void Render();
    public abstract void PostRender();

    public virtual void DebugRender() { }

    public virtual void Destroy() {
        IsDestroyed = true;
        Destroyed?.Invoke();
    }
}
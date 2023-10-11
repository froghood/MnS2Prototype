
using OpenTK.Mathematics;

namespace Touhou.Objects;

public abstract class Hitbox {

    public Entity Entity { get; }

    public Vector2 Offset { get; }
    public CollisionGroup CollisionGroup { get; }
    public Vector2 Position { get => Entity.Position + Offset; }



    private Action<Entity, Hitbox> collisionCallback;

    public Hitbox(Entity entity, Vector2 offset, CollisionGroup collisionGroup, Action<Entity, Hitbox> collisionCallback = default) {
        Entity = entity;
        Offset = offset;
        CollisionGroup = collisionGroup;
        this.collisionCallback = collisionCallback;
    }

    public abstract bool Check(PointHitbox other);
    public abstract bool Check(CircleHitbox other);
    public abstract bool Check(RectangleHitbox other);

    public void OnCollide(Entity entity, Hitbox hitbox) => collisionCallback?.Invoke(entity, hitbox);

    public abstract Box2 GetBounds();

    public virtual void Render() { }
}
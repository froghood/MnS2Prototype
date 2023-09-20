
using OpenTK.Mathematics;

namespace Touhou.Objects;

public abstract class Hitbox {

    public Entity Entity { get; }

    public Vector2 Offset { get; }
    public CollisionGroups CollisionGroup { get; }
    public Vector2 Position { get => Entity.Position + Offset; }



    private Action<Entity> collisionCallback;

    public Hitbox(Entity entity, Vector2 offset, CollisionGroups collisionGroup, Action<Entity> collisionCallback = default(Action<Entity>)) {
        Entity = entity;
        Offset = offset;
        CollisionGroup = collisionGroup;
        this.collisionCallback = collisionCallback;
    }

    public abstract bool Check(PointHitbox other);
    public abstract bool Check(CircleHitbox other);
    public abstract bool Check(RectangleHitbox other);

    public void Collide(Entity entity) => collisionCallback?.Invoke(entity);

    public abstract Box2 GetBounds();
}
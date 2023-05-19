using SFML.Graphics;
using SFML.System;

namespace Touhou.Objects;

public abstract class Hitbox {

    public Entity Entity { get; private set; }

    public Vector2f Offset { get; private set; }
    public Vector2f Position { get => Entity.Position + Offset; }

    private Action<Entity> collisionCallback;

    public Hitbox(Entity entity, Vector2f offset, Action<Entity> collisionCallback = default(Action<Entity>)) {
        Entity = entity;
        Offset = offset;
        this.collisionCallback = collisionCallback;
    }

    public abstract bool Check(PointHitbox other);
    public abstract bool Check(CircleHitbox other);
    public abstract bool Check(RectangleHitbox other);

    public void Collide(Entity entity) => collisionCallback?.Invoke(entity);

    public abstract FloatRect GetBounds();
}
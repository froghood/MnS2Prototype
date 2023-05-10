using SFML.Graphics;
using SFML.System;

namespace Touhou.Objects;

public abstract class Hitbox {

    public Entity Entity { get; private set; }

    public Vector2f Offset { get; private set; }
    public Vector2f Position { get => Entity.Position + Offset; }

    public Hitbox(Entity entity, Vector2f offset) {
        Entity = entity;
        Offset = offset;
    }

    public abstract bool Check(PointHitbox other);
    public abstract bool Check(CircleHitbox other);
    public abstract bool Check(RectangleHitbox other);

    public abstract FloatRect GetBounds();
}
using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class LargeKnife : TimestopProjectile {
    private float velocity;

    public LargeKnife(Vector2 origin, float direction, float velocity, bool doesStartFrozen, bool isPlayerOwned, bool isRemote) : base(origin, direction, doesStartFrozen, isPlayerOwned, isRemote) {

        this.velocity = velocity;

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, 3.75f, isPlayerOwned ? CollisionGroups.PlayerProjectile : CollisionGroups.OpponentProjectileMinor));

    }

    protected override Vector2 PositionFunction(float t) {
        return new Vector2(
            velocity * t,
            0f
        );
    }

    protected override float AngleFunction(float t) {

        var bounds = Match.Bounds;

        var directionVector = new Vector2(MathF.Cos(Orientation), MathF.Sin(Orientation));
        var distToVerticalWall = float.PositiveInfinity;
        var distToHorizontalWall = float.PositiveInfinity;
        if (directionVector.X != 0f) distToVerticalWall = (MathF.Sign(directionVector.X) * bounds.X - Origin.X) / directionVector.X;
        if (directionVector.Y != 0f) distToHorizontalWall = (MathF.Sign(directionVector.Y) * bounds.Y - Origin.Y) / directionVector.Y;

        var position = SamplePosition(t);

        if (distToHorizontalWall > distToVerticalWall) {
            if (MathF.Abs(position.X) > bounds.X) return MathF.PI - Orientation * 2f;
        } else {
            if (MathF.Abs(position.Y) > bounds.Y) return -Orientation - Orientation;
        }

        return 0f;

    }

    protected override Vector2 SecondaryPositionFunction(float time, Vector2 position) {

        var bounds = Match.Bounds; // - new Vector2(radius, radius);

        var originBoundsOffset = new Vector2(
            MathF.Max(MathF.Min(MathF.Floor((position.X + bounds.X) / (bounds.X * 2f)), 1f), -1f),
            MathF.Max(MathF.Min(MathF.Floor((position.Y + bounds.Y) / (bounds.Y * 2f)), 1f), -1f)
        );

        var directionVector = new Vector2(MathF.Cos(Orientation), MathF.Sin(Orientation));

        var distToVerticalWall = float.PositiveInfinity;
        var distToHorizontalWall = float.PositiveInfinity;

        if (directionVector.X != 0f) distToVerticalWall = (MathF.Sign(directionVector.X) * bounds.X - Origin.X) / directionVector.X;
        if (directionVector.Y != 0f) distToHorizontalWall = (MathF.Sign(directionVector.Y) * bounds.Y - Origin.Y) / directionVector.Y;

        if (distToHorizontalWall > distToVerticalWall) {
            return new Vector2(
                position.X - MathF.Abs(originBoundsOffset.X) * (position.X - originBoundsOffset.X * bounds.X) * 2f,
                position.Y// - MathF.Abs(originBoundsOffset.Y) * (position.Y - originBoundsOffset.Y * bounds.Y) * 2f
            );
        } else {
            return new Vector2(
                position.X,
                position.Y - MathF.Abs(originBoundsOffset.Y) * (position.Y - originBoundsOffset.Y * bounds.Y) * 2f
            );
        }
    }

    public override void Render() {

        float spawnTime = MathF.Min(LifeTime.AsSeconds() / SpawnDelay.AsSeconds(), 1f);

        var sprite = new Sprite("knife2") {

            Origin = new Vector2(0.5f),

            Position = Position,
            Rotation = Tangent,
            Scale = new Vector2(0.45f) * (1f + 3f * (1f - spawnTime)),

            Color = new Color4(
                Color.R,
                Color.G,
                Color.B,
                Color.A * spawnTime),

            UseColorSwapping = true,
        };

        Game.Draw(sprite, IsPlayerOwned ? Layers.PlayerProjectiles1 : Layers.OpponentProjectiles1);

        // Game.Draw(new Circle {
        //     Origin = new Vector2(0.5f),
        //     Position = Position,
        //     Radius = ((CircleHitbox)Hitboxes[0]).Radius,
        //     StrokeWidth = 1f,
        //     StrokeColor = new Color4(1f, 1f, 1f, 1f),
        //     FillColor = Color4.Transparent,

        // }, Layers.Foreground2);

        base.Render();
    }
}
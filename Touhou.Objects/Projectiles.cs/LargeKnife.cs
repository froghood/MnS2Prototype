using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class LargeKnife : TimestopProjectile {
    private float velocity;

    public LargeKnife(Vector2 origin, float direction, float velocity, bool doesStartFrozen, bool isPlayerOwned, bool isRemote) : base(origin, direction, doesStartFrozen, isPlayerOwned, isRemote) {

        this.velocity = velocity;

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, 6f, isPlayerOwned ? CollisionGroup.PlayerProjectile : CollisionGroup.OpponentProjectileMinor));

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


        float middle = (MathF.Max(MathF.Max(Color.R, Color.G), Color.B) + MathF.Min(MathF.Min(Color.R, Color.G), Color.B)) / 2f;
        float saturation = IsTimestopped ? 0.25f : 1f;

        var sprite = new Sprite("largeknife") {

            Origin = new Vector2(0.5f),

            Position = Position,
            Rotation = Tangent,
            Scale = new Vector2(0.48f) * (1f + 3f * (1f - spawnTime)),

            Color = new Color4(
                middle + (Color.R - middle) * saturation,
                middle + (Color.G - middle) * saturation,
                middle + (Color.B - middle) * saturation,
                Color.A * spawnTime),

            UseColorSwapping = true,
        };

        Game.Draw(sprite, IsPlayerOwned ? Layer.PlayerProjectiles : Layer.OpponentProjectiles);

    }
}
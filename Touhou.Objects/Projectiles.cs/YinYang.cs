
using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class YinYang : ParametricProjectile {


    public float Velocity { get; init; }

    private float radius;
    private float visualRadius;

    private readonly Sprite sprite;

    public YinYang(Vector2 origin, float direction, bool isP1Owned, bool isPlayerOwned, bool isRemote, float radius) : base(origin, direction, isP1Owned, isPlayerOwned, isRemote) {

        this.radius = radius;
        this.visualRadius = radius + 8f;

        Hitboxes.Add(new CircleHitbox(this, -new Vector2(0f, 0f), this.radius, isP1Owned ? CollisionGroup.P1MajorProjectile : CollisionGroup.P2MajorProjectile));

        sprite = new Sprite("yinyang") {
            Origin = new Vector2(0.5f),
            UseColorSwapping = true,
        };
    }

    public override void Render() {

        sprite.Position = Position;
        sprite.Scale = Vector2.One * visualRadius / 250f * Easing.Out(SpawnFactor, 3f);

        if (LifeTime >= SpawnDelay + SpawnDuration) sprite.Rotation = Velocity * LifeTime.AsSeconds() / visualRadius;

        sprite.Color = new Color4(
           Color.R,
           Color.G,
           Color.B,
           Color.A * Easing.Out(SpawnFactor, 3f));

        Game.Draw(sprite, IsPlayerOwned ? Layer.PlayerBackgroundProjectiles : Layer.OpponentBackgroundProjectiles);

    }


    protected override Vector2 PositionFunction(float t) {
        return new Vector2(Velocity * t, 0f);
    }



    protected override Vector2 SecondaryPositionFunction(float time, Vector2 position) {

        var adjustedMatchBounds = Match.Bounds; // - new Vector2(radius, radius);

        position += adjustedMatchBounds;

        var matchBounds = adjustedMatchBounds * 2f;

        var screenOffset = new Vector2(
            MathF.Abs(MathF.Floor(position.X / matchBounds.X)),
            MathF.Abs(MathF.Floor(position.Y / matchBounds.Y))
        );

        return new Vector2(
            (screenOffset.X % 2 * matchBounds.X + TMathF.Mod(position.X, matchBounds.X) * MathF.Pow(-1, screenOffset.X)) - adjustedMatchBounds.X,
            (screenOffset.Y % 2 * matchBounds.Y + TMathF.Mod(position.Y, matchBounds.Y) * MathF.Pow(-1, screenOffset.Y)) - adjustedMatchBounds.Y
        );
    }
}
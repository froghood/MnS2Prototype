
using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class YinYang : ParametricProjectile {


    public float Velocity { get; init; }

    private float radius;
    private float visualRadius;

    private readonly Sprite sprite;

    public YinYang(Vector2 origin, float direction, bool isPlayerOwned, bool isRemote, float radius, Time spawnTimeOffset = default(Time)) : base(origin, direction, isPlayerOwned, isRemote, spawnTimeOffset) {

        this.radius = radius;
        this.visualRadius = radius + 8f;

        Hitboxes.Add(new CircleHitbox(this, -new Vector2(0f, 0f), this.radius, isPlayerOwned ? CollisionGroups.PlayerProjectile : CollisionGroups.OpponentProjectileMajor));

        sprite = new Sprite("yinyang") {
            Origin = new Vector2(0.5f),
            UseColorSwapping = true,
        };
    }

    public override void Render() {


        float spawnRatio = MathF.Min(LifeTime.AsSeconds() / SpawnDelay.AsSeconds(), 1f);


        sprite.Position = Position;
        sprite.Scale = Vector2.One * visualRadius / 250f * Easing.Out(spawnRatio, 3f);

        if (LifeTime >= SpawnDelay) sprite.Rotation = Velocity * (LifeTime - SpawnDelay).AsSeconds() / visualRadius;

        sprite.Color = new Color4(
           Color.R,
           Color.G,
           Color.B,
           Color.A * Easing.Out(spawnRatio, 3f));

        Game.Draw(sprite, IsPlayerOwned ? Layers.PlayerProjectiles1 : Layers.OpponentProjectiles1);

        // Game.Draw(new Circle {
        //     Origin = new Vector2(0.5f),
        //     Position = Position,
        //     Radius = radius,
        //     StrokeWidth = 1f,
        //     StrokeColor = new Color4(0f, 0f, 1f, 1f),
        //     FillColor = Color4.Transparent,

        // }, Layers.Foreground2);
    }

    public override void DebugRender() {
        foreach (CircleHitbox hitbox in Hitboxes) {

            // var states = new CircleStates() {
            //     Origin = new Vector2(0.5f, 0.5f),
            //     Position = hitbox.Position,
            //     Radius = hitbox.Radius,
            //     FillColor4 = Color.Transparent,
            //     OutlineColor4 = Color.White,
            // };

            //Game.DrawCircle(states, Layers.UI2);

            // hitboxShape.Position = hitbox.Position;
            // hitboxShape.Radius = hitbox.Radius;
            // hitboxShape.Origin = new Vector2(hitbox.Radius, hitbox.Radius);
            // Game.Draw(hitboxShape, 0);

            // var bounds = hitbox.GetBounds();
            // boundsShape.Position = new Vector2(bounds.Left, bounds.Top);
            // boundsShape.Size = new Vector2(bounds.Width, bounds.Height);
            // Game.Draw(boundsShape, 0);


        }
    }

    protected override float FuncX(float t) {
        // float t = MathF.Max(time - 0.15f, 0f);
        return Velocity * t;
    }

    protected override float FuncY(float time) {
        return 0f;
    }

    protected override Vector2 Adjust(float time, Vector2 position) {

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
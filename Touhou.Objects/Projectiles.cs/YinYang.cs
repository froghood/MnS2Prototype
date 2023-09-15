
using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class YinYang : ParametricProjectile {


    public float Velocity { get; init; }

    private float radius;

    private readonly Sprite sprite;

    public YinYang(Vector2 origin, float direction, bool isPlayerOwned, bool isRemote, float radius, Time spawnTimeOffset = default(Time)) : base(origin, direction, isPlayerOwned, isRemote, spawnTimeOffset) {

        this.radius = radius;

        Hitboxes.Add(new CircleHitbox(this, -new Vector2(0f, 0f), this.radius, isPlayerOwned ? CollisionGroups.PlayerProjectile : CollisionGroups.OpponentProjectile));

        sprite = new Sprite("yinyang") {
            Origin = new Vector2(0.5f),
            Scale = new Vector2(0.25f, 0.25f) * radius / 60f,
            UseColorSwapping = true,
        };
    }

    public override void Render() {

        sprite.Position = Position;
        sprite.Rotation = CurrentTime * Velocity / radius;
        sprite.Scale = new Vector2(0.25f, 0.25f) * radius / 60f;
        sprite.Color = Color;

        Game.Draw(sprite, IsPlayerOwned ? Layers.PlayerProjectiles1 : Layers.OpponentProjectiles1);

        // circle.Radius = radius + 5f;
        // circle.Origin = new Vector2(circle.Radius, circle.Radius);
        // circle.FillColor4 = Color;
        // circle.Position = Position;

        // var state = new SpriteStates() {
        //     Origin = new Vector2(0.5f, 0.5f),
        //     Position = Position,
        //     Rotation = CurrentTime * Velocity / (MathF.Tau * radius) * -360f,
        //     Scale = new Vector2(0.25f, 0.25f) * radius / 60f
        // };

        // var shader = new TShader("projectileColor4");
        // shader.SetUniform("Color4", Color);

        //Game.DrawSprite("yinyang", state, shader, Layers.BackgroundProjectiles1);

        //Game.Draw(circle, 0);
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

        position += Match.Bounds;

        var matchBoundsX = Match.Bounds.X * 2f;
        var matchBoundsY = Match.Bounds.Y * 2f;

        var screenOffset = new Vector2(
            MathF.Abs(MathF.Floor(position.X / matchBoundsX)),
            MathF.Abs(MathF.Floor(position.Y / matchBoundsY))
        );

        return new Vector2(
            (screenOffset.X % 2 * matchBoundsX + TMathF.Mod(position.X, matchBoundsX) * MathF.Pow(-1, screenOffset.X)) - Match.Bounds.X,
            (screenOffset.Y % 2 * matchBoundsY + TMathF.Mod(position.Y, matchBoundsY) * MathF.Pow(-1, screenOffset.Y)) - Match.Bounds.Y
        );
    }
}
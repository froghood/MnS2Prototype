using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class Laser : Projectile {

    public Time SpawnDeley { get; init; }

    private readonly float direction;
    private readonly Vector2 directionVector;
    private readonly float width;
    private readonly Time startupTime;
    private readonly Time activeTime;
    private readonly float visualScale;
    private Time timeOffset;

    private Time time { get => LifeTime + timeOffset; }
    private Time timeWithSpawnDelay { get => Time.Max(time - SpawnDeley, 0L); }

    public Laser(Vector2 position, float direction, float width, Time startupTime, Time activeTime, bool isPlayerOwned, bool isRemote) : base(isPlayerOwned, isRemote) {
        Position = position;
        this.direction = direction;
        this.directionVector = new Vector2(MathF.Cos(direction), MathF.Sin(direction));
        this.width = width;
        this.startupTime = startupTime;
        this.activeTime = activeTime;

        visualScale = width / 600f;

    }

    public override void Init() {

        base.Init();

        var directionMatrix = Matrix2.CreateRotation(direction);

        var startHitbox = new CircleHitbox(
            this,
            new Vector2(width / 2f, 0f) * directionMatrix,
            width / 2f,
            IsPlayerOwned ? CollisionGroup.PlayerProjectile : CollisionGroup.OpponentProjectileMajor);

        Hitboxes.Add(startHitbox);

        int i = 0;
        while (true) {
            var hitbox = new RectangleHitbox(
                this,
                new Vector2(width * i + width, 0f) * directionMatrix,
                new Vector2(width),
                direction,
                IsPlayerOwned ? CollisionGroup.PlayerProjectile : CollisionGroup.OpponentProjectileMajor);

            var bounds = hitbox.GetBounds();

            if (bounds.Min.X >= Match.Bounds.X || bounds.Max.X <= -Match.Bounds.X ||
                bounds.Min.Y >= Match.Bounds.Y || bounds.Max.Y <= -Match.Bounds.Y) {
                break;
            }

            Hitboxes.Add(hitbox);
            i++;
        }
    }

    public override void Update() {

        CanCollide = timeWithSpawnDelay >= startupTime && timeWithSpawnDelay < startupTime + activeTime ? true : false;
        if (timeWithSpawnDelay >= startupTime + activeTime) Destroy(Time.InSeconds(0.25f));

        base.Update();
    }

    public override void Render() {



        if (time < SpawnDeley) return;

        if (timeWithSpawnDelay < startupTime) {
            var indicator = new Sprite("laser_indicator") {
                Origin = new Vector2(0f, 0.5f),
                Position = Position + directionVector * width / 2f,
                Scale = new Vector2(10000f, visualScale),
                Rotation = direction,
                Color = new Color4(
                    Color.R,
                    Color.G,
                    Color.B,
                    0.15f
                ),
                UseColorSwapping = true,
                BlendMode = BlendMode.Additive,
            };

            var indicatorStart = new Sprite(indicator) {
                SpriteName = "laser_indicator_start",
                Origin = new Vector2(1f, 0.5f),
                Scale = new Vector2(visualScale, visualScale)
            };


            var progressStart = new Sprite(indicatorStart) {
                SpriteName = "laser_indicator_progress_start",
                Scale = new Vector2(visualScale * MathF.Min(timeWithSpawnDelay.AsSeconds() / startupTime.AsSeconds(), 1f)),
                Color = new Color4(Color.R, Color.G, Color.B, 0.25f)
            };


            var progress = new Sprite(indicator) {
                SpriteName = "laser_indicator_progress",
                Scale = new Vector2(10000f, visualScale * MathF.Min(timeWithSpawnDelay.AsSeconds() / startupTime.AsSeconds(), 1f)),
                Color = new Color4(Color.R, Color.G, Color.B, 0.25f),
            };

            Game.Draw(indicatorStart, IsPlayerOwned ? Layer.PlayerProjectiles1 : Layer.OpponentProjectiles1);
            Game.Draw(indicator, IsPlayerOwned ? Layer.PlayerProjectiles1 : Layer.OpponentProjectiles1);

            Game.Draw(progressStart, IsPlayerOwned ? Layer.PlayerProjectiles1 : Layer.OpponentProjectiles1);
            Game.Draw(progress, IsPlayerOwned ? Layer.PlayerProjectiles1 : Layer.OpponentProjectiles1);


        } else {

            var laser = new Sprite("laser") {
                Origin = new Vector2(0f, 0.5f),
                Position = Position + directionVector * width / 2f,
                Scale = new Vector2(10000f, visualScale * (1f - Easing.In(DestroyedFactor, 3f))),
                Rotation = direction,
                Color = Color,
                UseColorSwapping = true,
                BlendMode = BlendMode.Additive,
            };

            var laserStart = new Sprite(laser) {
                SpriteName = "laser_start",
                Origin = new Vector2(1f, 0.5f),
                Scale = new Vector2(visualScale, visualScale * (1f - Easing.In(DestroyedFactor, 3f))),
            };

            Game.Draw(laserStart, IsPlayerOwned ? Layer.PlayerProjectiles1 : Layer.OpponentProjectiles1);
            Game.Draw(laser, IsPlayerOwned ? Layer.PlayerProjectiles1 : Layer.OpponentProjectiles1);

        }
    }

    public void SetTime(Time time) {
        timeOffset = -LifeTime + time;
    }

    public void FowardTime(Time time) {
        timeOffset += time;
    }





}
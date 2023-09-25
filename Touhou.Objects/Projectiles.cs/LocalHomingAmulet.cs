using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Net;

namespace Touhou.Objects.Projectiles;

public class LocalHomingAmulet : Homing {









    private Vector2 visualOffset;
    private float interpolationTime;



    private Sprite sprite;



    public LocalHomingAmulet(Vector2 position, float startingAngle, float turnRadius, float velocity, float hitboxRadius) : base(true, false) {
        Position = position;
        angle = startingAngle;
        visualRotation = startingAngle + MathF.PI / 2f;
        this.turnRadius = turnRadius;
        this.velocity = velocity;

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, hitboxRadius, CollisionGroups.PlayerProjectile));

        sprite = new Sprite("spinningamulet") {
            Origin = new Vector2(0.5f),
            UseColorSwapping = true,
        };
    }

    public override void Update() {


        var lifeTime = Game.Time - SpawnTime;

        if (state == HomingState.Spawning && lifeTime >= SpawnDuration) {
            state = HomingState.PreHoming;
        }

        if (state == HomingState.Spawning) return;

        if (state == HomingState.PreHoming) {
            Position += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * velocity * 2f * Game.Delta.AsSeconds();
            return;
        }

        if (state == HomingState.PostHoming) {
            Position += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * velocity * Game.Delta.AsSeconds();
            base.Update();
            return;
        }

        // homing
        if (side == 0) {
            Position += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * velocity * Game.Delta.AsSeconds();
        } else {
            var maxTurn = velocity * Game.Delta.AsSeconds() / (turnRadius * MathF.Tau) * MathF.Tau;

            angle += maxTurn * side;

            Position = turnPosition + new Vector2() {
                X = turnRadius * MathF.Cos(angle - MathF.PI / 2f * side),
                Y = turnRadius * MathF.Sin(angle - MathF.PI / 2f * side)
            };
        }
    }


    public override void Receive(Packet packet, IPEndPoint endPoint) {

        base.Receive(packet, endPoint);


        if (packet.Type != PacketType.UpdateProjectile) return;

        packet.Out(out uint id, true);

        if (id != Id) return;

        packet.Out(out Time theirTime).Out(out HomingState theirState);

        state = theirState;

        var visualPosition = Position + visualOffset * interpolationTime;

        if (state == HomingState.Homing) {
            packet.Out(out Vector2 theirPosition).Out(out float theirAngle).Out(out int theirSide);

            Position = theirPosition;
            angle = theirAngle;
            side = theirSide;

            var latency = Game.Network.Time - theirTime;

            if (side == 0) {
                Position += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * velocity * latency.AsSeconds();
            } else {
                turnPosition = Position + new Vector2() {
                    X = turnRadius * MathF.Cos(angle + MathF.PI / 2f * side),
                    Y = turnRadius * MathF.Sin(angle + MathF.PI / 2f * side)
                };

                var maxTurn = velocity * latency.AsSeconds() / (turnRadius * MathF.Tau) * MathF.Tau;
                angle += maxTurn * side;
                Position = turnPosition + new Vector2() {
                    X = turnRadius * MathF.Cos(angle - MathF.PI / 2f * side),
                    Y = turnRadius * MathF.Sin(angle - MathF.PI / 2f * side)
                };
            }
        } else {

            packet.Out(out Vector2 theirPosition).Out(out float theirAngle);

            Position = theirPosition;
            angle = theirAngle;

            var latency = Game.Network.Time - theirTime;

            Position += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * velocity * latency.AsSeconds();
        }

        visualOffset = visualPosition - Position;
        interpolationTime = 1f;
    }

    public override void Render() {

        var lifeTime = Game.Time - SpawnTime;

        bool spinning = (state == HomingState.PostHoming || state == HomingState.Homing);
        var spawnRatio = MathF.Min(1f, lifeTime.AsSeconds() / SpawnDuration.AsSeconds());

        if (spinning) visualRotation += MathF.Tau * Game.Delta.AsSeconds() * 2f;

        sprite.Position = Position + visualOffset * interpolationTime;
        sprite.Scale = Vector2.One * (spinning ? 0.40f : 0.35f) * (1f + 3f * (1f - spawnRatio));
        sprite.Rotation = visualRotation;

        sprite.Color = new Color4(
             Color.R,
             Color.G * (state == HomingState.PostHoming ? 0.7f : 1f),
             Color.B,
             Color.A * spawnRatio);

        Game.Draw(sprite, Layers.PlayerProjectiles1);

        base.Render();
    }

    public override void PostRender() {
        interpolationTime = MathF.Max(interpolationTime - Game.Delta.AsSeconds(), 0f);
    }

    public override void Destroy() {
        base.Destroy();

        System.Console.WriteLine("destroyed");
    }
}
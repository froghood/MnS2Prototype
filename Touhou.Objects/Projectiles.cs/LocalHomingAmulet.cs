using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;

namespace Touhou.Objects.Projectiles;

public class LocalHomingAmulet : Homing {









    private Vector2 visualOffset;
    private float interpolationTime;



    private Sprite sprite;



    public LocalHomingAmulet(Vector2 position, float startingAngle, float turnRadius, float velocity, float hitboxRadius, bool isP1Owned, bool isPlayerOwned) : base(isP1Owned, isPlayerOwned, false) {
        Position = position;
        angle = startingAngle;
        visualRotation = startingAngle + MathF.PI / 2f;
        this.turnRadius = turnRadius;
        this.velocity = velocity;

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, hitboxRadius, isP1Owned ? CollisionGroup.P1MinorProjectile : CollisionGroup.P2MinorProjectile));

        sprite = new Sprite("spinningamulet") {
            Origin = new Vector2(0.5f),
            UseColorSwapping = true,
        };

        Log.Info(isP1Owned);
    }

    public override void Update() {


        if (state == HomingState.Spawning && LifeTime >= SpawnDuration) {
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


    public override void Receive(Packet packet) {

        base.Receive(packet);


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

            var latency = Game.NetworkOld.Time - theirTime;

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

            var latency = Game.NetworkOld.Time - theirTime;

            Position += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * velocity * latency.AsSeconds();
        }

        visualOffset = visualPosition - Position;
        interpolationTime = 1f;
    }

    public override void Render() {

        bool spinning = (state == HomingState.PostHoming || state == HomingState.Homing);
        var spawnRatio = MathF.Min(1f, LifeTime.AsSeconds() / SpawnDuration.AsSeconds());

        if (spinning) visualRotation += MathF.Tau * Game.Delta.AsSeconds() * 2f;

        sprite.Position = Position + visualOffset * interpolationTime;
        sprite.Scale = Vector2.One * (spinning ? 0.40f : 0.35f) * (1f + 3f * (1f - spawnRatio));
        sprite.Rotation = visualRotation;

        sprite.Color = new Color4(
             Color.R,
             Color.G * (state == HomingState.PostHoming ? 0.7f : 1f),
             Color.B,
             Color.A * spawnRatio);

        Game.Draw(sprite, Layer.PlayerProjectiles);
    }

    public override void PostRender() {
        interpolationTime = MathF.Max(interpolationTime - Game.Delta.AsSeconds(), 0f);
    }
}
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Projectiles;

public class RemoteHomingAmulet : Homing {

    private float hitboxRadius;



    private int? lastSideChange = null;
    private Time nextPacketTimeThreshold;



    private Player Player => player is null ? player = Scene.GetFirstEntity<Player>() : player;
    private Player player;



    private Sprite sprite;



    public RemoteHomingAmulet(Vector2 position, float startAngle, float turnRadius, float velocity, float hitboxRadius, Time spawnTimeOffset = default(Time)) : base(false, true, spawnTimeOffset) {
        Position = position;
        angle = startAngle;
        visualRotation = startAngle + MathF.PI / 2f;
        this.turnRadius = turnRadius;
        this.velocity = velocity;
        this.hitboxRadius = hitboxRadius;

        System.Console.WriteLine($"{position}, {startAngle}, {turnRadius}, {velocity}, {hitboxRadius}, {spawnTimeOffset}");

        turnPosition = Position + new Vector2() {
            X = turnRadius * MathF.Cos(angle + MathF.PI / 2f),
            Y = turnRadius * MathF.Sin(angle + MathF.PI / 2f)
        };

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, hitboxRadius, CollisionGroups.OpponentProjectileMinor));

        sprite = new Sprite("spinningamulet") {
            Origin = new Vector2(0.5f),
            UseColorSwapping = true,
        };

    }



    public override void Update() {

        var lifeTime = Game.Time - SpawnTime;

        if (state == HomingState.Spawning && lifeTime >= SpawnDuration) {
            state = HomingState.PreHoming;

            Forward(velocity * 2f * MathF.Min(PreHomingDuration.AsSeconds(), lifeTime.AsSeconds() - SpawnDuration.AsSeconds()));
        }

        if (state == HomingState.PreHoming && lifeTime >= SpawnDuration + PreHomingDuration) {
            state = HomingState.Homing;
        }

        if (state == HomingState.Homing && lifeTime >= SpawnDuration + PreHomingDuration + HomingDuration) {
            state = HomingState.PostHoming;

            var packet = new Packet(PacketType.UpdateProjectile).In(Id ^ 0x80000000).In(Game.Network.Time).In(state).In(Position).In(angle);
            Game.Network.Send(packet);
        }

        if (state == HomingState.Spawning) return;


        if (state == HomingState.PreHoming) {
            Forward(velocity * 2f * Game.Delta.AsSeconds());
            return;
        }

        if (state == HomingState.PostHoming) {
            Forward(velocity * Game.Delta.AsSeconds());
            base.Update();
            return;
        }



        // homing
        var prevSide = side;

        var angleFromProjectileToPlayer = TMathF.NormalizeAngle(MathF.Atan2(Player.Position.Y - Position.Y, Player.Position.X - Position.X) - angle);

        // center
        if (side == 0) {

            var distFromProjectileToPlayer = MathF.Sqrt(MathF.Pow(Player.Position.X - Position.X, 2f) + MathF.Pow(Player.Position.Y - Position.Y, 2f));

            var opposite = Math.Abs(distFromProjectileToPlayer * MathF.Sin(angleFromProjectileToPlayer));

            if (opposite > hitboxRadius || MathF.Abs(angleFromProjectileToPlayer) > MathF.PI / 2f) { // switch to turning
                side = MathF.Sign(angleFromProjectileToPlayer);
                turnPosition = Position + new Vector2() {
                    X = turnRadius * MathF.Cos(angle + MathF.PI / 2f * (int)side),
                    Y = turnRadius * MathF.Sin(angle + MathF.PI / 2f * (int)side)
                };
            } else {
                Forward(velocity * Game.Delta.AsSeconds());
            }
        }

        // not center
        if (side != 0) Turn();

        // prevents excessive packet spam
        if (side != prevSide) lastSideChange = side;
        if (lastSideChange.HasValue && lifeTime >= nextPacketTimeThreshold) {
            var packet = new Packet(PacketType.UpdateProjectile).In(Id ^ 0x80000000).In(Game.Network.Time).In(state).In(Position).In(angle).In(side);
            Game.Network.Send(packet);

            lastSideChange = null;
            nextPacketTimeThreshold = lifeTime + Time.InSeconds(0.2f);//Game.Random.NextSingle() * 0.1f);
        }

        void Forward(float distance) {
            Position += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
        }

        void Turn() {

            var targetSide = MathF.Sign(angleFromProjectileToPlayer);

            if (targetSide != side) {

                side = -side;

                turnPosition = Position + new Vector2() {
                    X = turnRadius * MathF.Cos(angle + MathF.PI / 2f * (int)side),
                    Y = turnRadius * MathF.Sin(angle + MathF.PI / 2f * (int)side)
                };
            }

            var distFromTurnCenterToPlayer = MathF.Sqrt(MathF.Pow(Player.Position.X - turnPosition.X, 2f) + MathF.Pow(Player.Position.Y - turnPosition.Y, 2f));
            var ratio = turnRadius / distFromTurnCenterToPlayer;

            var angleFromTurnCenterToPlayer = MathF.Atan2(Player.Position.Y - turnPosition.Y, Player.Position.X - turnPosition.X);
            var targetTangentAngle = TMathF.NormalizeAngle(MathF.Asin(ratio) * (int)side + angleFromTurnCenterToPlayer); // -Pi : Pi
            var arcLengthToTarget = TMathF.Mod((targetTangentAngle - angle) * (int)side, MathF.Tau); // 0 : Tau
            var maxTurn = velocity * Game.Delta.AsSeconds() / (turnRadius * MathF.Tau) * MathF.Tau;

            if (arcLengthToTarget <= maxTurn) {
                angle += arcLengthToTarget * side;

                Position = turnPosition + new Vector2() {
                    X = turnRadius * MathF.Cos(angle - MathF.PI / 2f * side),
                    Y = turnRadius * MathF.Sin(angle - MathF.PI / 2f * side)
                };

                side = 0; // switch to forward

                // travel forward remaining distance
                var remainingTravel = (maxTurn - arcLengthToTarget) * turnRadius;
                Forward(remainingTravel);

            } else {
                angle += maxTurn * side;

                Position = turnPosition + new Vector2() {
                    X = turnRadius * MathF.Cos(angle - MathF.PI / 2f * side),
                    Y = turnRadius * MathF.Sin(angle - MathF.PI / 2f * side)
                };
            }
        }


    }

    public override void Render() {
        var lifeTime = Game.Time - SpawnTime;

        bool spinning = (state == HomingState.PostHoming || state == HomingState.Homing);
        var spawnRatio = MathF.Min(1f, lifeTime.AsSeconds() / SpawnDuration.AsSeconds());

        if (spinning) visualRotation += MathF.Tau * Game.Delta.AsSeconds() * 2f;

        sprite.Position = Position;
        sprite.Scale = Vector2.One * (spinning ? 0.40f : 0.35f) * (1f + 3f * (1f - spawnRatio));
        sprite.Rotation = visualRotation;

        sprite.Color = new Color4(
            Color.R * (state == HomingState.PostHoming ? 0.7f : 1f),
            Color.G,
            Color.B,
            Color.A * spawnRatio);

        Game.Draw(sprite, Layers.OpponentProjectiles1);

        base.Render();

        // var states = new SpriteStates() {
        //     Origin = new Vector2(0.5f, 0.5f),
        //     Position = Position,
        //     Rotation = rotation,
        //     Scale = new Vector2(1f, 1f) * 0.4f
        // };

        //var Color4 = isHoming ? Color4 : new Color4(170, 0, 200);

        //var shader = new TShader("projectileColor4");
        //shader.SetUniform("Color4", Color4);

        //Game.DrawSprite("spinningamulet", states, shader, Layers.Projectiles2);


        // if (side == 0 || !isHoming) return;

        // circle.Radius = turnRadius;
        // circle.Position = turnPosition;
        // circle.Origin = new Vector2(1f, 1f) * circle.Radius;
        // circle.FillColor4 = Color4.Transparent;
        // circle.OutlineColor4 = new Color4(255, 255, 255, 30);
        // circle.OutlineThickness = 1f;
        // Game.Draw(circle, 0);
    }
}
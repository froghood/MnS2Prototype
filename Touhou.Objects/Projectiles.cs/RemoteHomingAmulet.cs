using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Net;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Projectiles;

public class RemoteHomingAmulet : Projectile {

    private readonly float turnRadius;
    private readonly float velocity;
    private readonly float hitboxRadius;
    private float angle;
    private bool isHoming = true;
    private int side = 0;
    private Vector2 turnPosition;

    private float rotation;



    private int? bufferedSideChange = null;
    private Time nextPacketTimeThreshold;



    private Player Player => player is null ? player = Scene.GetFirstEntity<Player>() : player;
    private Player player;
    private Sprite sprite;

    public RemoteHomingAmulet(Vector2 position, float startAngle, float turnRadius, float velocity, float hitboxRadius, Time spawnTimeOffset = default(Time)) : base(false, true, spawnTimeOffset) {
        Position = position;
        angle = startAngle;
        this.turnRadius = turnRadius;
        this.velocity = velocity;
        this.hitboxRadius = hitboxRadius;

        System.Console.WriteLine($"{position}, {startAngle}, {turnRadius}, {velocity}, {hitboxRadius}, {spawnTimeOffset}");

        turnPosition = Position + new Vector2() {
            X = turnRadius * MathF.Cos(angle + MathF.PI / 2f),
            Y = turnRadius * MathF.Sin(angle + MathF.PI / 2f)
        };

        // rect = new RectangleShape(new Vector2(25f, 22f));
        // rect.Rotation = Game.Random.NextSingle() * 180f;
        // rect.Origin = rect.Size / 2f;

        // circle = new CircleShape();

        Hitboxes.Add(new CircleHitbox(this, new Vector2(0f, 0f), hitboxRadius, CollisionGroups.OpponentProjectileMinor));

        sprite = new Sprite("spinningamulet") {
            Origin = new Vector2(0.5f, 0.5f),
            Rotation = rotation,
            Scale = new Vector2(1f, 1f) * 0.45f,
            UseColorSwapping = true,
        };

    }



    public override void Update() {

        var lifeTime = Game.Time - SpawnTime;
        if (lifeTime < Time.InSeconds(0.25f)) return;

        if (isHoming && Game.Time >= SpawnTime + Time.InSeconds(4f)) {
            isHoming = false;

            var packet = new Packet(PacketType.UpdateProjectile).In(Id ^ 0x80000000).In(false).In(Game.Network.Time).In(Position).In(angle);
            Game.Network.Send(packet);
        }

        if (!isHoming) {
            Forward(velocity * Game.Delta.AsSeconds());
            base.Update();
            return;
        }

        var prevSide = side;

        if (side == 0) {
            var distFromProjectileToPlayer = MathF.Sqrt(MathF.Pow(Player.Position.X - Position.X, 2f) + MathF.Pow(Player.Position.Y - Position.Y, 2f));
            var angleFromProjectileToPlayer = MathF.Atan2(Player.Position.Y - Position.Y, Player.Position.X - Position.X);

            var a = TMathF.NormalizeAngle(angleFromProjectileToPlayer - angle);
            var opposite = Math.Abs(distFromProjectileToPlayer * MathF.Sin(a));

            if (opposite > hitboxRadius || MathF.Abs(a) > MathF.PI / 2f) { // switch to turning
                side = MathF.Sign(a);
                turnPosition = Position + new Vector2() {
                    X = turnRadius * MathF.Cos(angle + MathF.PI / 2f * side),
                    Y = turnRadius * MathF.Sin(angle + MathF.PI / 2f * side)
                };
            } else {
                Forward(velocity * Game.Delta.AsSeconds());
            }
        }

        if (side != 0) {
            Turn();
        }

        if (side != prevSide) bufferedSideChange = side;

        if (bufferedSideChange.HasValue && lifeTime >= nextPacketTimeThreshold) {
            var packet = new Packet(PacketType.UpdateProjectile).In(Id ^ 0x80000000).In(true).In(Game.Network.Time).In(Position).In(angle).In(side);
            Game.Network.Send(packet);

            bufferedSideChange = null;
            nextPacketTimeThreshold = lifeTime + Time.InSeconds(0.2f);//Game.Random.NextSingle() * 0.1f);
        }

        //base.Update();

        void Turn() {

            var angleFromProjectileToPlayer = MathF.Atan2(Player.Position.Y - Position.Y, Player.Position.X - Position.X);
            var targetSide = MathF.Sign(TMathF.NormalizeAngle(angleFromProjectileToPlayer - angle));

            if (targetSide != side) {
                side = -side;
                turnPosition = Position + new Vector2() {
                    X = turnRadius * MathF.Cos(angle + MathF.PI / 2f * side),
                    Y = turnRadius * MathF.Sin(angle + MathF.PI / 2f * side)
                };
            }

            var distFromTurnCenterToPlayer = MathF.Sqrt(MathF.Pow(Player.Position.X - turnPosition.X, 2f) + MathF.Pow(Player.Position.Y - turnPosition.Y, 2f));
            var ratio = turnRadius / distFromTurnCenterToPlayer;

            // if (ratio > 1) {
            //     isHoming = false;
            //     Forward(velocity * Game.Delta.AsSeconds());
            //     base.Update();
            //     return;
            // }

            var angleFromTurnCenterToPlayer = MathF.Atan2(Player.Position.Y - turnPosition.Y, Player.Position.X - turnPosition.X);
            var targetTangentAngle = TMathF.NormalizeAngle(MathF.Asin(ratio) * side + angleFromTurnCenterToPlayer); // -Pi : Pi
            var arcLengthToTarget = TMathF.Mod((targetTangentAngle - angle) * side, MathF.Tau); // 0 : Tau
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

        void Forward(float distance) {
            Position += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
        }
    }

    public override void Render() {
        // rect.FillColor4 = isHoming ? Color4 : new Color4(170, 0, 200);
        // rect.Position = Position;
        // rect.Rotation += 360f * Game.Delta.AsSeconds() * 2f;
        // Game.Draw(rect, 0);

        rotation += MathF.Tau * Game.Delta.AsSeconds() * 2f;

        sprite.Position = Position;
        sprite.Rotation = rotation;
        sprite.Color = isHoming ? Color : new Color4(170, 0, 200, 255);

        Game.Draw(sprite, Layers.PlayerProjectiles1);

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
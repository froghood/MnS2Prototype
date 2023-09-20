using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Net;

namespace Touhou.Objects.Projectiles;

public class LocalHomingAmulet : Projectile {



    private readonly float turnRadius;
    private readonly float velocity;

    private float angle;
    private int side = 0;
    private Vector2 turnPosition;



    private Vector2 visualOffset;
    private float interpolationTime;
    private bool isHoming = true;
    private bool hasStartedMoving;
    private float rotation;



    private Sprite sprite;



    public LocalHomingAmulet(Vector2 position, float startingAngle, float turnRadius, float velocity, float hitboxRadius) : base(true, false) {
        Position = position;
        angle = startingAngle;
        this.turnRadius = turnRadius;
        this.velocity = velocity;


        Hitboxes.Add(new CircleHitbox(this, new Vector2(0f, 0f), hitboxRadius, CollisionGroups.PlayerProjectile));

        rotation = Game.Random.NextSingle() * 360f;

        sprite = new Sprite("spinningamulet") {
            Origin = new Vector2(0.5f, 0.5f),
            Rotation = rotation,
            Scale = new Vector2(1f, 1f) * 0.45f,
            UseColorSwapping = true,
        };
    }

    public override void Update() {


        var lifeTime = Game.Time - SpawnTime;


        if (!hasStartedMoving && lifeTime < Time.InSeconds(0.25f)) return;


        if (!isHoming) {
            Position += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * velocity * Game.Delta.AsSeconds();
            base.Update();
            return;
        }


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

        packet.Out(out bool theirIsHoming);

        var visualPosition = Position + visualOffset * interpolationTime;

        if (theirIsHoming) {
            packet.Out(out Time theirTime).Out(out Vector2 theirPosition).Out(out float theirAngle).Out(out int theirSide);

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
            isHoming = false;

            packet.Out(out Time theirTime).Out(out Vector2 theirPosition).Out(out float theirAngle);





            Position = theirPosition;
            angle = theirAngle;

            var latency = Game.Network.Time - theirTime;

            System.Console.WriteLine($"{latency},{theirPosition}, {theirAngle}");

            Position += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * velocity * latency.AsSeconds();
        }

        visualOffset = visualPosition - Position;
        interpolationTime = 1f;

        hasStartedMoving = true;
    }

    public override void Render() {

        rotation += MathF.Tau * Game.Delta.AsSeconds() * 2f;

        sprite.Position = Position + visualOffset * interpolationTime;
        sprite.Rotation = rotation;
        sprite.Color = Color;

        Game.Draw(sprite, Layers.PlayerProjectiles1);

        base.Render();


        // var states = new SpriteStates() {
        //     Origin = new Vector2(0.5f, 0.5f),
        //     Position = Position + visualOffset * interpolationTime,
        //     Rotation = rotation,
        //     Scale = new Vector2(1f, 1f) * 0.4f
        // };


        //var shader = new TShader("projectileColor4");
        //shader.SetUniform("Color4", Color);

        //Game.DrawSprite("spinningamulet", states, shader, Layers.Projectiles1);


        // if (side == 0) return;

        // circle.Radius = turnRadius;
        // circle.Position = turnPosition;
        // circle.Origin = new Vector2(1f, 1f) * circle.Radius;
        // circle.FillColor4 = Color4.Transparent;
        // circle.OutlineColor4 = new Color4(255, 255, 255, 30);
        // circle.OutlineThickness = 1f;
        // Game.Draw(circle, 0);
    }

    public override void PostRender() {
        interpolationTime = MathF.Max(interpolationTime - Game.Delta.AsSeconds(), 0f);
    }

    public override void Destroy() {
        base.Destroy();

        System.Console.WriteLine("destroyed");
    }
}
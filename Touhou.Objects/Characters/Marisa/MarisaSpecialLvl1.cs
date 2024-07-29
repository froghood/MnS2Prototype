using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public partial class Marisa : Character {


    private class SpecialLvl1 : Attack<Marisa> {

        private readonly int cost = 80;
        private readonly float velocity = 450f;
        private readonly float trailVelocity = 22f;
        private readonly int grazeAmount = 8;
        private readonly int trailGrazeAmount = 2;
        private bool isHeld;
        private float aimWeight;
        private float spawnPositionX;
        private float spawnAngle;


        public SpecialLvl1(Marisa c) : base(c) {
            IsHoldable = true;
        }

        public override void LocalPress(Time cooldownOverflow, bool focused) {

            isHeld = true;

            aimWeight = 0f;

            C.DisableAttacks(
                PlayerActions.Primary,
                PlayerActions.Secondary,
                PlayerActions.Charge
            );




        }



        public override void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) {
            var isMoving = (C.Velocity.X != 0f || C.Velocity.Y != 0f);

            if (isMoving) {
                aimWeight *= MathF.Pow(0.3f, Game.Delta.AsSeconds());
                aimWeight += C.Velocity.Normalized().X * Game.Delta.AsSeconds();

            } else {
                aimWeight *= MathF.Pow(0.1f, Game.Delta.AsSeconds());
            }

            spawnPositionX = C.Opponent.Position.X + aimWeight * 450f;
            spawnAngle = -MathF.PI / 2f + aimWeight * 1f;
        }



        public override void LocalRelease(Time cooldownOverflow, Time heldTime, bool focused) {
            var seed = Game.Random.Next();

            var spawnPosition = new Vector2(spawnPositionX, C.Match.Bounds.Y);

            var shootingStar = new ShootingStar(seed, velocity, trailVelocity, spawnPosition, spawnAngle, C.IsP1, C.IsPlayer, false) {
                SpawnDuration = Time.InSeconds(0.25f),
                CanCollide = false,
                Color = new Color4(0f, 1f, 0f, 0.4f),
            };
            C.Scene.AddEntity(shootingStar);

            shootingStar.ForwardTime(cooldownOverflow, false);



            var packet = new Packet(PacketType.AttackReleased)
            .In(PlayerActions.Special)
            .In(Game.Network.Time - cooldownOverflow)
            .In(spawnPositionX)
            .In(spawnAngle)
            .In(seed);

            Game.Network.Send(packet);

            isHeld = false;


            C.ApplyAbilityLock(Time.InSeconds(0.2f) - cooldownOverflow,
                PlayerActions.Primary,
                PlayerActions.Secondary,
                PlayerActions.Special,
                PlayerActions.Charge
            );

            C.EnableAttacks(
                PlayerActions.Primary,
                PlayerActions.Secondary,
                PlayerActions.Charge
            );
        }



        public override void RemoteRelease(Packet packet) {
            packet
            .Out(out Time theirTime)
            .Out(out float theirPositionX)
            .Out(out float theirAngle)
            .Out(out int theirSeed);

            var latency = Game.Network.Time - theirTime;

            var spawnPosition = new Vector2(theirPositionX, C.Match.Bounds.Y);

            var shootingStar = new ShootingStar(theirSeed, velocity, trailVelocity, spawnPosition, theirAngle, C.IsP1, C.IsPlayer, true) {
                SpawnDuration = Time.InSeconds(0.25f),
                Color = new Color4(1f, 0f, 0f, 1f),
                GrazeAmount = grazeAmount,
                TrailGrazeAmount = trailGrazeAmount,
            };
            C.Scene.AddEntity(shootingStar);

            shootingStar.ForwardTime(latency, true);


        }

        public override void Render() {

            if (!isHeld) return;

            var preview = new Sprite("fade") {
                Origin = new Vector2(0f, 0.5f),
                Position = new Vector2(spawnPositionX, C.Match.Bounds.Y),
                Rotation = spawnAngle,
                Scale = new Vector2(6f, 0.08f),
                Color = new Color4(1f, 1f, 1f, 0.2f),
            };

            var positionPreview = new Circle() {
                Origin = new Vector2(0.5f),
                Position = new Vector2(spawnPositionX, C.Match.Bounds.Y),
                Radius = 10f,
                StrokeWidth = 4f,
                StrokeColor = new Color4(1f, 1f, 1f, 0.5f),
                FillColor = Color4.Transparent,
            };



            Game.Draw(preview, Layer.Player);
            Game.Draw(positionPreview, Layer.Player);
        }
    }
}
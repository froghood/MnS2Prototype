using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Projectiles;

public class RemoteHomingAmulet : Homing {

    private float hitboxRadius;



    private int? lastSideChange = null;
    private Time nextPacketTimeThreshold;



    private Character character { get => _character ??= Scene.GetFirstEntityWhere<Character>(e => e.IsP1 != IsP1Owned); }
    private Character _character;


    private Sprite sprite;



    public RemoteHomingAmulet(Vector2 position, float startAngle, float turnRadius, float velocity, float hitboxRadius, bool isP1Owned, bool isPlayerOwned) : base(isP1Owned, isPlayerOwned, true) {
        Position = position;
        angle = startAngle;
        visualRotation = startAngle + MathF.PI / 2f;
        this.turnRadius = turnRadius;
        this.velocity = velocity;
        this.hitboxRadius = hitboxRadius;

        Log.Info($"{position}, {startAngle}, {turnRadius}, {velocity}, {hitboxRadius}");

        turnPosition = Position + new Vector2() {
            X = turnRadius * MathF.Cos(angle + MathF.PI / 2f),
            Y = turnRadius * MathF.Sin(angle + MathF.PI / 2f)
        };

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

            Forward(velocity * 2f * MathF.Min(PreHomingDuration.AsSeconds(), LifeTime.AsSeconds() - SpawnDuration.AsSeconds()));
        }

        if (state == HomingState.PreHoming && LifeTime >= SpawnDuration + PreHomingDuration) {
            state = HomingState.Homing;
        }

        if (state == HomingState.Homing && LifeTime >= SpawnDuration + PreHomingDuration + HomingDuration) {
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

        var angleFromProjectileToCharacter = TMathF.NormalizeAngle(MathF.Atan2(character.Position.Y - Position.Y, character.Position.X - Position.X) - angle);

        // center
        if (side == 0) {

            var distFromProjectileToCharacter = MathF.Sqrt(MathF.Pow(character.Position.X - Position.X, 2f) + MathF.Pow(character.Position.Y - Position.Y, 2f));

            var opposite = Math.Abs(distFromProjectileToCharacter * MathF.Sin(angleFromProjectileToCharacter));

            if (opposite > hitboxRadius || MathF.Abs(angleFromProjectileToCharacter) > MathF.PI / 2f) { // switch to turning
                side = MathF.Sign(angleFromProjectileToCharacter);
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
        if (lastSideChange.HasValue && LifeTime >= nextPacketTimeThreshold) {
            var packet = new Packet(PacketType.UpdateProjectile).In(Id ^ 0x80000000).In(Game.Network.Time).In(state).In(Position).In(angle).In(side);
            Game.Network.Send(packet);

            lastSideChange = null;
            nextPacketTimeThreshold = LifeTime + Time.InSeconds(0.2f);//Game.Random.NextSingle() * 0.1f);
        }

        void Forward(float distance) {
            Position += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
        }

        void Turn() {

            var targetSide = MathF.Sign(angleFromProjectileToCharacter);

            if (targetSide != side) {

                side = -side;

                turnPosition = Position + new Vector2() {
                    X = turnRadius * MathF.Cos(angle + MathF.PI / 2f * (int)side),
                    Y = turnRadius * MathF.Sin(angle + MathF.PI / 2f * (int)side)
                };
            }

            var distFromTurnCenterToCharacter = MathF.Sqrt(MathF.Pow(character.Position.X - turnPosition.X, 2f) + MathF.Pow(character.Position.Y - turnPosition.Y, 2f));
            var ratio = turnRadius / distFromTurnCenterToCharacter;

            var angleFromTurnCenterToCharacter = MathF.Atan2(character.Position.Y - turnPosition.Y, character.Position.X - turnPosition.X);
            var targetTangentAngle = TMathF.NormalizeAngle(MathF.Asin(ratio) * (int)side + angleFromTurnCenterToCharacter); // -Pi : Pi
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

        bool spinning = (state == HomingState.PostHoming || state == HomingState.Homing);
        var spawnRatio = MathF.Min(1f, LifeTime.AsSeconds() / SpawnDuration.AsSeconds());

        if (spinning) visualRotation += MathF.Tau * Game.Delta.AsSeconds() * 2f;

        sprite.Position = Position;
        sprite.Scale = Vector2.One * (spinning ? 0.40f : 0.35f) * (1f + 3f * (1f - spawnRatio));
        sprite.Rotation = visualRotation;

        sprite.Color = new Color4(
            Color.R * (state == HomingState.PostHoming ? 0.7f : 1f),
            Color.G,
            Color.B,
            Color.A * spawnRatio);

        Game.Draw(sprite, Layer.OpponentProjectiles);

        base.Render();

    }
}
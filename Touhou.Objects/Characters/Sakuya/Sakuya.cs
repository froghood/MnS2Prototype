using OpenTK.Mathematics;
using Touhou.Debugging;
using Touhou.Graphics;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public class Sakuya : Character {

    public bool IsTimestopped { get; private set; }
    public Queue<TimestopProjectile> TimestoppedProjectiles { get; private set; } = new();
    public Timer TimestopTimer { get; private set; }

    public int TimestopSpendCost { get; private set; }
    public Time TimestopSpendTime { get; set; }


    public Sakuya(bool isP1, bool isPlayer, Color4 color) : base(isP1, isPlayer, color) {

        Speed = 350f;
        FocusedSpeed = 250f;

        InitMoveset(
            new SakuyaPrimary(this),
            new SakuyaSecondary(this),
            new SakuyaSpecial(this),
            new SakuyaSuper(this),
            new SakuyaBomb(this)
        );

    }

    public void EnableTimestop(int cost) {
        IsTimestopped = true;
        TimestopTimer = new Timer();
        TimestopSpendCost = cost;
        TimestopSpendTime = 0L;
    }

    public void DisableTimestop(Time timeIncrease, bool interpolate) {

        IsTimestopped = false;
        while (TimestoppedProjectiles.Count > 0) {

            Log.Warn(TimestoppedProjectiles.Count);

            TimestoppedProjectiles.Dequeue().Unfreeze(timeIncrease, interpolate);
        }
    }

    public override void Render() {

        if (State == CharacterState.Dead) return;

        var sprite = new Sprite("sakuya") {
            Origin = new Vector2(0.45f, 0.32f),
            Position = Position,
            Scale = new Vector2(MathF.Sign(Position.X - Opponent.Position.X), 1f) * 0.2f,
            Color = Color,
            UseColorSwapping = false,
        };

        Game.Draw(sprite, IsPlayer ? Layer.Player : Layer.Opponent);

        base.Render();
    }

    public override Entity GetController(ControllerType type) {
        return (type) switch {
            ControllerType.LocalNetplay => new LocalSakuyaController(this),
            ControllerType.RemoteNetplay => new RemoteCharacterController<Character>(this),
            _ => throw new Exception($"Character controller does not exist for controller type {type}")
        };
    }
}

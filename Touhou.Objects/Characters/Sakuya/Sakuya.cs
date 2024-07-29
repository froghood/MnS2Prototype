using OpenTK.Mathematics;
using Touhou.Debugging;
using Touhou.Graphics;
using Touhou.Objects.Projectiles;

namespace Touhou.Objects.Characters;

public partial class Sakuya : Character {

    public bool IsTimestopped { get; private set; }
    public Queue<TimestopProjectile> TimestoppedProjectiles { get; private set; } = new();
    public Timer TimestopTimer { get; private set; }

    public int TimestopSpendCost { get; private set; }
    public Time TimestopSpendTime { get; set; }


    public Sakuya(bool isP1, bool isPlayer, Color4 color) : base(isP1, isPlayer, color) {

        Speed = 350f;
        FocusedSpeed = 250f;

        // TODO: implement other attack levels
        Primary = new Ability(
            new PrimaryLvl1(this),
            new PrimaryLvl2(this),
            new PrimaryLvl2(this)
        );

        Secondary = new Ability(
            new SecondaryLvl1(this),
            new SecondaryLvl1(this),
            new SecondaryLvl1(this)
        );

        Special = new Ability(
            new SpecialLvl1(this),
            new SpecialLvl1(this),
            new SpecialLvl1(this)
        );

    }

    public void EnableTimestop(Time duration) {
        IsTimestopped = true;
        TimestopTimer = new Timer(duration);
    }

    public void DisableTimestop(Time timeIncrease, bool interpolate) {

        IsTimestopped = false;

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

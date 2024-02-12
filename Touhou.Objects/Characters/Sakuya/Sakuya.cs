using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class Sakuya : Character {
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

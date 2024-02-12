using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class Marisa : Character {
    public Marisa(bool isP1, bool isPlayer, Color4 color) : base(isP1, isPlayer, color) {

        Speed = 350f;
        FocusedSpeed = 150f;

        InitMoveset(
            new MarisaPrimary(this),
            new MarisaSecondary(this),
            new MarisaSpecial(this),
            new MarisaSuper(this),
            new MarisaBomb(this)
        );
    }

    public override void Render() {

        if (State == CharacterState.Dead) return;

        var sprite = new Sprite("marisa") {
            Origin = new Vector2(0.45f, 0.18f),
            Position = Position,
            Scale = new Vector2(MathF.Sign(Position.X - Opponent.Position.X), 1f) * 0.26f,
            Color = Color,
            UseColorSwapping = false,
        };

        Game.Draw(sprite, IsPlayer ? Layer.Player : Layer.Opponent);

        base.Render();
    }
}
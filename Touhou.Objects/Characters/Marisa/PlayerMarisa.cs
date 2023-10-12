using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class PlayerMarisa : Player {
    public PlayerMarisa(bool isP1) : base(isP1) {

        Speed = 350f;
        FocusedSpeed = 150f;

        AddAttack(PlayerActions.Primary, new MarisaPrimary());
        AddAttack(PlayerActions.Secondary, new MarisaSecondary());
        AddAttack(PlayerActions.SpecialA, new MarisaSpecialA());
        AddAttack(PlayerActions.SpecialB, new MarisaSpecialB());

        AddBomb(new ReimuBomb());
    }

    public override void Render() {

        if (IsDead) return;

        var sprite = new Sprite("marisa") {
            Origin = new Vector2(0.45f, 0.2f),
            Position = Position,
            Scale = new Vector2(MathF.Sign(Position.X - Opponent.Position.X), 1f) * 0.26f,
            Color = Color,
            UseColorSwapping = false,
        };

        Game.Draw(sprite, Layer.Player);

        base.Render();
    }
}
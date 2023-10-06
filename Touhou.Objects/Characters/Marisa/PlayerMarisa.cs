using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class PlayerMarisa : Player {
    public PlayerMarisa(bool isP1) : base(isP1) {

        Speed = 350f;
        FocusedSpeed = 125f;

        AddAttack(PlayerActions.Primary, new ReimuPrimary());
        AddAttack(PlayerActions.Secondary, new ReimuSecondary());
        AddAttack(PlayerActions.SpecialA, new MarisaSpecialA());
        AddAttack(PlayerActions.SpecialB, new ReimuSpecialB());

        AddBomb(PlayerActions.Bomb, new ReimuBomb());
    }

    public override void Render() {

        if (IsDead) return;

        var sprite = new Sprite("marisa") {
            Origin = new Vector2(0.45f, 0.35f),
            Position = Position,
            Scale = new Vector2(MathF.Sign(Position.X - Opponent.Position.X), 1f) * 0.2f,
            Color = Color,
            UseColorSwapping = false,
        };

        Game.Draw(sprite, Layers.Player);

        base.Render();
    }
}
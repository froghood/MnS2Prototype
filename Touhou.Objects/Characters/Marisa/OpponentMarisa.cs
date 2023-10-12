using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects;

namespace Touhou.Objects.Characters;

public class OpponentMarisa : Opponent {

    public OpponentMarisa(bool isP1) : base(isP1) {

        AddAttack(PlayerActions.Primary, new MarisaPrimary());
        AddAttack(PlayerActions.Secondary, new MarisaSecondary());
        AddAttack(PlayerActions.SpecialA, new MarisaSpecialA());
        AddAttack(PlayerActions.SpecialB, new MarisaSpecialB());

        AddBomb(new ReimuBomb());
    }

    public override void Render() {

        if (IsDead) return;

        var sprite = new Sprite("marisa") {
            Origin = new Vector2(0.45f, 0.18f),
            Position = Position,
            Scale = new Vector2(MathF.Sign(Position.X - Player.Position.X), 1f) * 0.26f,
            Color = Color,
            UseColorSwapping = false,
        };

        Game.Draw(sprite, Layer.Opponent);

        base.Render();
    }
}
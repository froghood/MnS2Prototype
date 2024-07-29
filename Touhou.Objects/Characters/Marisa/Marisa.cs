using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public partial class Marisa : Character {
    public Marisa(bool isP1, bool isPlayer, Color4 color) : base(isP1, isPlayer, color) {

        Speed = 350f;
        FocusedSpeed = 150f;

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
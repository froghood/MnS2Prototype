using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public partial class Reimu : Character {



    public Reimu(bool isP1, bool isPlayer, Color4 color) : base(isP1, isPlayer, color) {

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

        var invulnColor = InvulnerabilityTimer.HasFinished ? 1f : (-MathF.Cos(InvulnerabilityTimer.TotalElapsed.AsSeconds() * MathF.Tau * 4f) + 1f) / 2 + 1f;


        string spriteName = (MathF.Sign(Velocity.X) * MathF.Sign(Opponent.Position.X - Position.X)) switch {
            -1 => "reimu0000",
            0 => "reimu0001",
            1 => "reimu0002",
            _ => ""
        };

        var sprite = new Sprite(spriteName) {
            Origin = new Vector2(0.45f, 0.45f),
            Position = Position,
            Scale = new Vector2(MathF.Sign(Position.X - Opponent.Position.X), 1f) * 0.09f,
            Color = new Color4(Color.R * invulnColor, Color.G * invulnColor, Color.B * invulnColor, 1f),
            UseColorSwapping = false,
        };

        Game.Draw(sprite, IsPlayer ? Layer.Player : Layer.Opponent);

        base.Render();
    }
}
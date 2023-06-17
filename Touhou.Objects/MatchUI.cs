using SFML.Graphics;
using SFML.System;
using Touhou.Objects.Characters;

namespace Touhou.Objects;

public class MatchUI : Entity {

    private Player Player => player is null ? Scene.GetFirstEntity<Player>() : player;
    private Player player;

    private Opponent Opponent => opponent is null ? Scene.GetFirstEntity<Opponent>() : opponent;
    private Opponent opponent;

    private Match Match => match is null ? Scene.GetFirstEntity<Match>() : match;
    private Match match;

    private float playerSmoothPower;
    private bool isHosting;
    private float opponentSmoothPower;

    public MatchUI(bool isHosting) {
        this.isHosting = isHosting;
    }

    public override void Render() {

        bool isP1 = isHosting ? true : false;

        RenderCooldowns();

        RenderPower(isP1, true);
        RenderPower(!isP1, false);

        RenderHearts(isP1, true);
        RenderHearts(!isP1, false);

        RenderTimer();
    }



    private void RenderCooldowns(bool isP1, bool isPlayer) {

        int i = isP1 ? 0 : 3;
        foreach (var (name, attack) in isPlayer ? Player.Attacks : Opponent.Attacks) {
            var states = new RectangeStates() {
                Origin = new Vector2f((isP1 ? -0.15f - 1.15f * i : 1.15f + 1.15f * i), 1.15f),
                Position = new Vector2f(isP1 ? 0f : 1f, 1f),
                Size = new Vector2f(120f, 120f),
                OutlineColor = Color.Transparent,
                FillColor = (isPlayer ? Player.Power : Opponent.Power) < attack.Cost ? new Color(230, 180, 190) : Color.White,
                IsUI = true,
            };

            var shader = new TShader("cooldown");
            shader.SetUniform("duration", attack.Cooldown.AsSeconds() / attack.CooldownDuration.AsSeconds());
            shader.SetUniform("disabled", attack.Disabled);

            Game.DrawRectangle(states, shader, 0);
            i += isP1 ? 1 : -1;
        }
    }

    private void RenderCooldowns() {

        int i = 0;
        foreach (var (name, attack) in Player.Attacks) {
            var states = new RectangeStates() {
                Origin = new Vector2f((0.5f - 1.15f * i) + 1.15f * (Player.Attacks.Count - 1) / 2f, 1.30f),
                Position = new Vector2f(0.5f, 1f),
                Size = new Vector2f(120f, 120f),
                OutlineColor = Color.Transparent,
                FillColor = Player.Power < attack.Cost ? new Color(230, 180, 190) : Color.White,
                IsUI = true,
            };

            var shader = new TShader("cooldown");
            shader.SetUniform("duration", attack.Cooldown.AsSeconds() / attack.CooldownDuration.AsSeconds());
            shader.SetUniform("disabled", attack.Disabled);

            Game.DrawRectangle(states, shader, 0);
            i++;
        }
    }



    private void RenderPower(bool isP1, bool isPlayer) {

        float maxWidth = 534f;
        float height = 18f;

        var bgStates = new RectangeStates() {
            Origin = new Vector2f((isP1 ? -18f : maxWidth + 18f) / maxWidth, 174f / height),
            Position = new Vector2f(isP1 ? 0f : 1f, 1f),
            Size = new Vector2f(maxWidth, height),
            FillColor = new Color(255, 255, 255, 80),
            OutlineColor = Color.Transparent,
            IsUI = true
        };

        Game.DrawRectangle(bgStates, 0);

        var width = (isPlayer ? Player.Power : Opponent.Power) / 400f * maxWidth;

        var states = new RectangeStates() {
            Origin = new Vector2f((isP1 ? -18f : maxWidth + 18f) / width, 174f / height),
            Position = new Vector2f(isP1 ? 0f : 1f, 1f),
            Size = new Vector2f(width, height),
            OutlineColor = Color.Transparent,
            IsUI = true
        };

        Game.DrawRectangle(states, 0);

        if (isPlayer) playerSmoothPower += MathF.Min(MathF.Abs(Player.Power - playerSmoothPower), Game.Delta.AsSeconds() * 80f) * MathF.Sign(Player.Power - playerSmoothPower);
        else opponentSmoothPower += MathF.Min(MathF.Abs(Opponent.Power - opponentSmoothPower), Game.Delta.AsSeconds() * 80f) * MathF.Sign(Opponent.Power - opponentSmoothPower);

        float smoothWidth = -((isPlayer ? Player.Power : Opponent.Power) - (isPlayer ? playerSmoothPower : opponentSmoothPower)) / 400f * maxWidth;

        var sStates = new RectangeStates() {
            Origin = new Vector2f(((isP1 ? -18f : maxWidth + 18f) - width) / smoothWidth, 174f / height),
            Position = new Vector2f(isP1 ? 0f : 1f, 1f),
            Size = new Vector2f(smoothWidth, height),
            FillColor = new Color(255, 200, 120),
            OutlineColor = Color.Transparent,
            IsUI = true
        };

        Game.DrawRectangle(sStates, 0);




    }



    private void RenderHearts(bool isP1, bool isPlayer) {

        for (int i = 0; i < (isPlayer ? Player.HeartCount : Opponent.HeartCount); i++) {
            var state = new SpriteStates() {
                Origin = new Vector2f(isP1 ? -0.8f - 1.15f * i : 1.8f + 1.15f * i, -0.8f),
                Position = new Vector2f(isP1 ? 0f : 1f, 0f),
                IsUI = true,
            };

            Game.DrawSprite("heart", state, Layers.UI1);
        }


    }

    private void RenderTimer() {
        float displayTime = MathF.Max(MathF.Ceiling((Match.EndTime - Match.StartTime - Match.CurrentTime).AsSeconds()), 0f);

        var timerStates = new TextStates() {
            CharacterSize = 100f,
            Style = Text.Styles.Bold,
            Origin = new Vector2f(0.5f, -0.08f),
            Position = new Vector2f(0.5f, 0f),
        };

        Game.DrawText(displayTime.ToString(), Game.DefaultFont, timerStates, Layers.UI1);

        var ppsStates = new TextStates() {
            CharacterSize = 50f,
            Style = Text.Styles.Bold,
            Origin = new Vector2f(0.5f, -3f),
            Position = new Vector2f(0.5f, 0f)
        };

        Game.DrawText($"{Match.GetPowerPerSecond()}/s", Game.DefaultFont, ppsStates, Layers.UI1);

    }

}
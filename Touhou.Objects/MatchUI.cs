using OpenTK.Mathematics;
using Touhou.Graphics;
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

        RenderSmallPower(true);
        RenderSmallPower(false);

        RenderSmallBombs(true);
        RenderSmallBombs(false);

        RenderHearts(isP1, true);
        RenderHearts(!isP1, false);

        RenderTimer();
    }



    private void RenderCooldowns(bool isP1, bool isPlayer) {

        int i = isP1 ? 0 : 3;

        foreach (var (name, attack) in isPlayer ? Player.Attacks : Opponent.Attacks) {


            var rectangle = new Rectangle {
                Origin = new Vector2((isP1 ? -0.15f - 1.15f * i : 1.15f + 1.15f * i), 1.15f),
                Position = new Vector2(isP1 ? 0f : 1f, 1f),
                Size = new Vector2(120f, 120f),
                StrokeColor = Color4.Black,
                StrokeWidth = 1f,
                FillColor = (isPlayer ? Player.Power : Opponent.Power) < attack.Cost ? new Color4(230, 180, 190, 255) : Color4.White,
                IsUI = true,
            };

            Game.Draw(rectangle, Layer.UI1);

            i += isP1 ? 1 : -1;
        }
    }

    private void RenderCooldowns() {

        var attacks = Player.Attacks.ToArray();

        int i = 0;
        foreach (var (action, attack) in attacks) {

            var fillColor = Player.Power < attack.Cost ? new Color4(230, 180, 190, 255) : (attack.Disabled | !attack.CooldownTimer.HasFinished ? Color4.Gray : Color4.White);



            var rectangle = new Rectangle {
                Origin = new Vector2(0.5f),
                Position = new Vector2(150f * i - (attacks.Length - 1) * 140f, 80f),
                Size = new Vector2(136f, 136f),
                StrokeColor = Player.Color,
                StrokeWidth = 4f,
                FillColor = Color4.Black,
                IsUI = true,
                Alignment = new Vector2(0f, attack.Disabled | !attack.CooldownTimer.HasFinished ? -1.01f : -1f),
            };

            Game.Draw(rectangle, Layer.UI1);





            string iconName;

            if (attack.Focusable) { // A

                if (Player.IsAttackHeld(attack, out var heldState)) { // B

                    iconName = heldState.Focused ? attack.FocusedIcon : attack.Icon; // C

                } else {
                    if (Game.Input.IsActionPressBuffered(action, out var bufferTime, out var bufferState)) {

                        iconName = bufferTime >= attack.CooldownTimer.FinishTime - Time.InSeconds(0.3f) // E
                            && bufferState.HasFlag(PlayerActions.Focus) ? attack.FocusedIcon : attack.Icon;

                    } else {

                        iconName = Game.IsActionPressed(PlayerActions.Focus) ? attack.FocusedIcon : attack.Icon; // F
                    }
                }
            } else {

                iconName = attack.Icon;
            }





            if (iconName is not null) {

                var icon = new Sprite(iconName) {
                    Origin = new Vector2(0.5f),
                    Position = new Vector2(150f * i - (attacks.Length - 1) * 140f, 80f),
                    IsUI = true,
                    Alignment = new Vector2(0f, attack.Disabled | !attack.CooldownTimer.HasFinished ? -1.01f : -1f),
                    Color = Player.Color,
                    UseColorSwapping = true,
                };

                Game.Draw(icon, Layer.UI1);
            }



            i++;
        }
    }



    private void RenderPower(bool isP1, bool isPlayer) {

        float maxWidth = 534f;
        float height = 18f;

        var bgRect = new Rectangle() {
            Origin = new Vector2((isP1 ? -18f : maxWidth + 18f) / maxWidth, -1f),
            Size = new Vector2(maxWidth, height),
            FillColor = new Color4(255, 255, 255, 80),
            StrokeColor = Color4.Transparent,
            IsUI = true,
            Alignment = new Vector2(isP1 ? -1 : 1, -1),

        };



        var width = (isPlayer ? Player.Power : Opponent.Power) / 400f * maxWidth;

        var rect = new Rectangle() {
            Origin = new Vector2((isP1 ? -18f : maxWidth + 18f) / width, -1f),
            Size = new Vector2(width, height),
            FillColor = Color4.White,
            StrokeColor = Color4.Transparent,
            IsUI = true,
            Alignment = new Vector2(isP1 ? -1 : 1, -1),
        };



        if (isPlayer) playerSmoothPower += MathF.Min(MathF.Abs(Player.Power - playerSmoothPower), Game.Delta.AsSeconds() * 80f) * MathF.Sign(Player.Power - playerSmoothPower);
        else opponentSmoothPower += MathF.Min(MathF.Abs(Opponent.Power - opponentSmoothPower), Game.Delta.AsSeconds() * 80f) * MathF.Sign(Opponent.Power - opponentSmoothPower);

        float smoothWidth = -((isPlayer ? Player.Power : Opponent.Power) - (isPlayer ? playerSmoothPower : opponentSmoothPower)) / 400f * maxWidth;

        var sRect = new Rectangle() {
            Origin = new Vector2(((isP1 ? -18f : maxWidth + 18f) - width) / smoothWidth, -1f),
            Size = new Vector2(smoothWidth, height),
            FillColor = new Color4(255, 200, 120, 255),
            StrokeColor = Color4.Transparent,
            IsUI = true,
            Alignment = new Vector2(isP1 ? -1 : 1, -1),
        };

        Game.Draw(bgRect, Layer.UI1);
        Game.Draw(rect, Layer.UI1);
        Game.Draw(sRect, Layer.UI1);

    }


    private void RenderSmallPower(bool isPlayer) {

        bool isDead = isPlayer ? Player.IsDead : Opponent.IsDead;

        if (isDead) return;

        var barWidth = 50f;

        int power = (isPlayer ? Player.Power : Opponent.Power);
        var position = (isPlayer ? Player.Position : Opponent.Position);

        var powerBarBG = new Rectangle {
            Size = new Vector2(barWidth, 6f),
            FillColor = new Color4(1f, 1f, 1f, 0.1f),
            StrokeColor = new Color4(1f, 1f, 1f, 0.2f),
            StrokeWidth = 1f,
            Origin = new Vector2(0f, 0.5f),
            Position = position + new Vector2(barWidth / -2f, -30f),
        };

        var powerBar = new Rectangle(powerBarBG) {
            Size = new Vector2(power / 400f * barWidth, 5f),
            FillColor = new Color4(1f, 1f, 1f, 0.3f),
            StrokeWidth = 0f,
        };

        Game.Draw(powerBarBG, Layer.UI1);
        Game.Draw(powerBar, Layer.UI1);
    }



    private void RenderSmallBombs(bool isPlayer) {

        bool isDead = isPlayer ? Player.IsDead : Opponent.IsDead;

        if (isDead) return;

        float spacing = 8f;

        int bombCount = isPlayer ? Player.BombCount : Opponent.BombCount;
        var position = isPlayer ? Player.Position : Opponent.Position;

        for (int i = 0; i < bombCount; i++) {
            var circle = new Circle {
                Radius = 2f,
                FillColor = new Color4(1f, 1f, 1f, 0.5f),
                Origin = new Vector2(0.5f),
                Position = position + new Vector2(spacing * i - (bombCount - 1) * spacing / 2f, -38f),
            };

            Game.Draw(circle, Layer.UI1);
        }
    }



    private void RenderHearts(bool isP1, bool isPlayer) {

        for (int i = 0; i < (isPlayer ? Player.HeartCount : Opponent.HeartCount); i++) {

            var sprite = new Sprite("heart") {
                Origin = new Vector2(isP1 ? -0.8f - 1.15f * i : 1.8f + 1.15f * i, -0.8f),
                Alignment = new Vector2(isP1 ? -1f : 1f, -1f),
                IsUI = true,
            };

            Game.Draw(sprite, Layer.UI1);
        }

    }

    private void RenderTimer() {
        float displayTime = MathF.Max(MathF.Ceiling((Match.EndTime - Match.StartTime - Match.CurrentTime).AsSeconds()), 0f);

        var timerText = new Text() {
            Origin = new Vector2(0.5f, 1f),
            IsUI = true,
            Alignment = new Vector2(0f, 1.02f),
            DisplayedText = displayTime.ToString(),
            Font = "consolas",
            CharacterSize = 120f,
            Boldness = 0.25f,
        };

        Game.Draw(timerText, Layer.UI1);

        var ppsText = new Text() {
            Origin = new Vector2(0.5f, 1f),
            //Position = new Vector2(0f, -116f),
            IsUI = true,
            Alignment = new Vector2(0f, 0.9f),
            DisplayedText = $"{Match.GetPowerPerSecond()}/s",
            Font = "consolas",
            CharacterSize = 60f,
            Boldness = 0.25f,


        };

        Game.Draw(ppsText, Layer.UI1);
    }

}
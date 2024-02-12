using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Objects.Characters;

namespace Touhou.Objects;

public class NetplayMatchUI : Entity {

    private Character p1C { get => _p1C ??= Scene.GetFirstEntityWhere<Character>(e => e.IsP1); }
    private Character _p1C;

    private Character p2C { get => _p2C ??= Scene.GetFirstEntityWhere<Character>(e => !e.IsP1); }
    private Character _p2C;

    private Character localC { get => _localC ??= Scene.GetFirstEntityWhere<Character>(e => e.IsP1 == isP1); }

    private Character _localC;

    private Match Match => match is null ? Scene.GetFirstEntity<Match>() : match;
    private Match match;

    private float playerSmoothPower;
    private bool isP1;
    private float opponentSmoothPower;

    public NetplayMatchUI(bool isP1) {
        this.isP1 = isP1;
    }

    public override void Render() {

        bool isP1 = this.isP1 ? true : false;

        //RenderCooldowns();

        // RenderPower(isP1, true);
        // RenderPower(!isP1, false);

        // RenderSmallPower(true);
        // RenderSmallPower(false);

        // RenderSmallBombs(true);
        // RenderSmallBombs(false);

        // RenderHearts(isP1, true);
        // RenderHearts(!isP1, false);

        //RenderTimer();
    }



    // private void RenderCooldowns(bool isP1, bool isPlayer) {

    //     int i = isP1 ? 0 : 3;

    //     foreach (var (name, attack) in isPlayer ? p1C.Attacks : p2C.Attacks) {


    //         var rectangle = new Rectangle {
    //             Origin = new Vector2((isP1 ? -0.15f - 1.15f * i : 1.15f + 1.15f * i), 1.15f),
    //             Position = new Vector2(isP1 ? 0f : 1f, 1f),
    //             Size = new Vector2(120f, 120f),
    //             StrokeColor = Color4.Black,
    //             StrokeWidth = 1f,
    //             FillColor = (isPlayer ? p1C.Power : p2C.Power) < attack.Cost ? new Color4(230, 180, 190, 255) : Color4.White,
    //             IsUI = true,
    //         };

    //         Game.Draw(rectangle, Layer.UI1);

    //         i += isP1 ? 1 : -1;
    //     }
    // }

    private void RenderCooldowns() {


        var attackActions = Enum.GetValues(typeof(PlayerActions)).OfType<PlayerActions>().Where(e => (e & (PlayerActions.Primary | PlayerActions.Secondary | PlayerActions.Special | PlayerActions.Super)) != 0).ToArray();

        int i = 0;
        foreach (var action in attackActions) {

            var attackCost = localC.GetAttackCost(action);

            var fillColor = localC.Power < attackCost ? new Color4(230, 180, 190, 255) : (localC.IsAttackAvailable(action) ? Color4.White : Color4.Gray);



            var rectangle = new Rectangle {
                Origin = new Vector2(0.5f),
                Position = new Vector2(150f * i - (attackActions.Length - 1) / 2f * 140f, 80f),
                Size = new Vector2(136f, 136f),
                StrokeColor = localC.Color,
                StrokeWidth = 4f,
                FillColor = Color4.Black,
                IsUI = true,
                Alignment = new Vector2(0f, localC.IsAttackAvailable(action) ? -1f : -1.01f),
            };

            Game.Draw(rectangle, Layer.UI1);





            string iconName;

            if (localC.IsAttackFocusable(action)) {

                if (localC.GetAttackHoldState(action, out var heldState)) {

                    iconName = localC.GetAttackIconName(action, heldState.IsFocused);

                } else {
                    if (Game.Input.IsActionPressBuffered(action, out var bufferTime, out var bufferState)) {

                        iconName = localC.GetAttackIconName(action,
                            bufferTime >= localC.GetAttackCooldownTimer(action).FinishTime - Time.InSeconds(0.3f) // buffer time
                            && bufferState.HasFlag(PlayerActions.Focus)
                        );


                    } else {

                        iconName = localC.GetAttackIconName(action, Game.IsActionPressed(PlayerActions.Focus));
                    }
                }
            } else {

                iconName = localC.GetAttackIconName(action, false);
            }

            if (iconName is not null) {

                var icon = new Sprite(iconName) {
                    Origin = new Vector2(0.5f),
                    Position = new Vector2(150f * i - (attackActions.Length - 1) / 2f * 140f, 80f),
                    IsUI = true,
                    Alignment = new Vector2(0f, localC.IsAttackAvailable(action) ? -1f : -1.01f),
                    Color = localC.Color,
                    UseColorSwapping = true,
                };

                Game.Draw(icon, Layer.UI1);
            }



            i++;
        }
    }



    // private void RenderPower(bool isP1, bool isPlayer) {

    //     float maxWidth = 534f;
    //     float height = 18f;

    //     var bgRect = new Rectangle() {
    //         Origin = new Vector2((isP1 ? -18f : maxWidth + 18f) / maxWidth, -1f),
    //         Size = new Vector2(maxWidth, height),
    //         FillColor = new Color4(255, 255, 255, 80),
    //         StrokeColor = Color4.Transparent,
    //         IsUI = true,
    //         Alignment = new Vector2(isP1 ? -1 : 1, -1),

    //     };



    //     var width = (isPlayer ? p1C.Power : p2C.Power) / 400f * maxWidth;

    //     var rect = new Rectangle() {
    //         Origin = new Vector2((isP1 ? -18f : maxWidth + 18f) / width, -1f),
    //         Size = new Vector2(width, height),
    //         FillColor = Color4.White,
    //         StrokeColor = Color4.Transparent,
    //         IsUI = true,
    //         Alignment = new Vector2(isP1 ? -1 : 1, -1),
    //     };



    //     if (isPlayer) playerSmoothPower += MathF.Min(MathF.Abs(p1C.Power - playerSmoothPower), Game.Delta.AsSeconds() * 80f) * MathF.Sign(p1C.Power - playerSmoothPower);
    //     else opponentSmoothPower += MathF.Min(MathF.Abs(p2C.Power - opponentSmoothPower), Game.Delta.AsSeconds() * 80f) * MathF.Sign(p2C.Power - opponentSmoothPower);

    //     float smoothWidth = -((isPlayer ? p1C.Power : p2C.Power) - (isPlayer ? playerSmoothPower : opponentSmoothPower)) / 400f * maxWidth;

    //     var sRect = new Rectangle() {
    //         Origin = new Vector2(((isP1 ? -18f : maxWidth + 18f) - width) / smoothWidth, -1f),
    //         Size = new Vector2(smoothWidth, height),
    //         FillColor = new Color4(255, 200, 120, 255),
    //         StrokeColor = Color4.Transparent,
    //         IsUI = true,
    //         Alignment = new Vector2(isP1 ? -1 : 1, -1),
    //     };

    //     Game.Draw(bgRect, Layer.UI1);
    //     Game.Draw(rect, Layer.UI1);
    //     Game.Draw(sRect, Layer.UI1);

    // }


    // private void RenderSmallPower(bool isP1) {

    //     bool isDead = isP1 ? p1C.IsDead : p2C.IsDead;

    //     if (isDead) return;

    //     var barWidth = 50f;

    //     int power = (isP1 ? p1C.Power : p2C.Power);
    //     var position = (isP1 ? p1C.Position : p2C.Position);

    //     var powerBarBG = new Rectangle {
    //         Size = new Vector2(barWidth, 6f),
    //         FillColor = new Color4(1f, 1f, 1f, 0.1f),
    //         StrokeColor = new Color4(1f, 1f, 1f, 0.2f),
    //         StrokeWidth = 1f,
    //         Origin = new Vector2(0f, 0.5f),
    //         Position = position + new Vector2(barWidth / -2f, -30f),
    //     };

    //     var powerBar = new Rectangle(powerBarBG) {
    //         Size = new Vector2(power / 400f * barWidth, 5f),
    //         FillColor = new Color4(1f, 1f, 1f, 0.3f),
    //         StrokeWidth = 0f,
    //     };

    //     Game.Draw(powerBarBG, Layer.UI1);
    //     Game.Draw(powerBar, Layer.UI1);
    // }



    // private void RenderSmallBombs(bool isPlayer) {

    //     bool isDead = isPlayer ? p1C.IsDead : p2C.IsDead;

    //     if (isDead) return;

    //     float spacing = 8f;

    //     int bombCount = isPlayer ? p1C.BombCount : p2C.BombCount;
    //     var position = isPlayer ? p1C.Position : p2C.Position;

    //     for (int i = 0; i < bombCount; i++) {
    //         var circle = new Circle {
    //             Radius = 2f,
    //             FillColor = new Color4(1f, 1f, 1f, 0.5f),
    //             Origin = new Vector2(0.5f),
    //             Position = position + new Vector2(spacing * i - (bombCount - 1) * spacing / 2f, -38f),
    //         };

    //         Game.Draw(circle, Layer.UI1);
    //     }
    // }



    // private void RenderHearts(bool isP1, bool isPlayer) {

    //     for (int i = 0; i < (isPlayer ? p1C.HeartCount : p2C.HeartCount); i++) {

    //         var sprite = new Sprite("heart") {
    //             Origin = new Vector2(isP1 ? -0.8f - 1.15f * i : 1.8f + 1.15f * i, -0.8f),
    //             Alignment = new Vector2(isP1 ? -1f : 1f, -1f),
    //             IsUI = true,
    //         };

    //         Game.Draw(sprite, Layer.UI1);
    //     }

    // }

    private void RenderTimer() {

        float displayTime = MathF.Max(MathF.Ceiling((Match.EndTime - Match.StartTime - Time.Max(Match.CurrentTime, 0L)).AsSeconds()), 0f);

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
            DisplayedText = $"{Match.CurrentPowerPerSecond}/s",
            Font = "consolas",
            CharacterSize = 60f,
            Boldness = 0.25f,


        };

        Game.Draw(ppsText, Layer.UI1);
    }

}
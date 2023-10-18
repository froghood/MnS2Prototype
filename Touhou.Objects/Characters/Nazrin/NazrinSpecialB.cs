using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;

namespace Touhou.Objects.Characters;

public class NazrinSpecialB : Attack {
    private bool isHolding;
    private bool isCanceled;
    private int currentCount;
    private long countIncreaseThreshold;
    private Time holdTime;
    private float renderRatio;

    public NazrinSpecialB() {
        Holdable = true;

        Cost = 20;

    }


    public override void PlayerPress(Player player, Time cooldownOverflow, bool focused) {


        isHolding = true;

        isCanceled = false;


        currentCount = 0;
        countIncreaseThreshold = Time.InSeconds(0.5f);

        renderRatio = 1f;

        player.ApplyMovespeedModifier(0.25f);



    }


    public override void PlayerHold(Player player, Time cooldownOverflow, Time holdTime, bool focused) {

        this.holdTime = holdTime;


        while (holdTime >= countIncreaseThreshold) {
            countIncreaseThreshold += Time.InSeconds(0.5f);
            currentCount++;

            player.SpendPower(Cost);

            if (player.Power < Cost) {
                player.ReleaseHeldAttack(PlayerActions.SpecialB);
                isCanceled = true;
                break;
            }
        }
    }



    public override void PlayerRelease(Player player, Time cooldownOverflow, Time heldTime, bool focused) {

        if (player is not PlayerNazrin nazrin) return;

        nazrin.SpawnMouse(currentCount);

        Log.Info($"Spawning {currentCount} {(currentCount == 1 ? "mouse" : "mice")}");

        player.ApplyAttackCooldowns(Time.InSeconds(currentCount > 0 ? 3F : 0.25f) - cooldownOverflow, PlayerActions.SpecialB);


        var packet = new Packet(PacketType.AttackReleased)
        .In(PlayerActions.SpecialB)
        .In(Game.Network.Time)
        .In(currentCount);

        Game.Network.Send(packet);

        isHolding = false;

        player.ApplyMovespeedModifier(1f);
    }



    public override void OpponentReleased(Opponent opponent, Packet packet) {

        Log.Info(opponent.GetType());

        if (opponent is not OpponentNazrin nazrin) return;

        packet
        .Out(out Time theirTime)
        .Out(out int theirCount);

        Log.Info($"Spawning mice: {theirCount}");

        var latency = Game.Network.Time - theirTime;

        nazrin.SpawnMouse(theirCount);


    }

    public override void PlayerRender(Player player) {


        var position = player.Position + new Vector2(0f, 80f);




        var fadeScale = isHolding ? (holdTime.AsSeconds() / 0.5f) % 1f : 1f;

        var scale = (1f - Easing.In(renderRatio, 3f)) * 0.5f + 1f;
        var color = isCanceled ? new Color4(1f, 0.8f, 0.8f, renderRatio) : new Color4(1f, 1f, 1f, renderRatio);


        var fade = new Sprite("circle_fade") {
            Origin = new Vector2(0.5f),
            Position = position,
            Scale = new Vector2(0.6f * fadeScale * scale),
            Color = new Color4(color.R, color.G, color.B, color.A * 0.4f),
        };

        var circle = new Circle {
            Radius = 30f * scale,
            FillColor = Color4.Transparent,
            StrokeColor = new Color4(color.R, color.G, color.B, color.A * 0.4f),
            StrokeWidth = 2f,
            Origin = new Vector2(0.5f),
            Position = position,
        };

        var text = new Text {
            DisplayedText = $"{currentCount}",
            Font = "consolas",
            CharacterSize = 24f,
            Color = color,
            Origin = new Vector2(0.5f, 0.35f),
            Position = position,
            Scale = new Vector2(scale),
        };

        Game.Draw(fade, Layer.Player);
        Game.Draw(circle, Layer.Player);
        Game.Draw(text, Layer.Player);

        if (!isHolding) renderRatio = MathF.Max(renderRatio - Game.Delta.AsSeconds() * 2f, 0f);
    }
}
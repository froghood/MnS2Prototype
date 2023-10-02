using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects;

public class TimestopVFX : Entity {

    private Func<Vector2> positionFunction;
    private Layers layer;
    private bool destroyed;
    private Time destroyedTime;



    public TimestopVFX(Func<Vector2> positionFunction, Layers layer) {
        this.positionFunction = positionFunction;
        this.layer = layer;
    }

    public override void Update() {
        if (destroyed && LifeTime - destroyedTime >= Time.InSeconds(1f)) base.Destroy();
    }

    public override void Render() {

        float fadeInFactor = Easing.Out(MathF.Min(LifeTime.AsSeconds() * 3f, 1f), 3f);
        float fadeOutFactor = destroyed ? Easing.Out(MathF.Min((LifeTime - destroyedTime).AsSeconds() * 3f, 1f), 3f) : 0f;

        float scale = ((1f - fadeInFactor) + fadeOutFactor) / 2f + 0.25f;

        var sprite = new Sprite("timestop") {
            Origin = new Vector2(0.5f),
            Position = positionFunction?.Invoke() ?? Vector2.Zero,
            Scale = new Vector2(scale),
            Color = new Color4(
                1f, 1f, 1f,
                0.2f * (fadeInFactor - fadeOutFactor)
            ),
        };

        Game.Draw(sprite, layer);
    }

    public override void Destroy() {
        destroyed = true;
        destroyedTime = LifeTime;
    }
}
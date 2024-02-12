using OpenTK.Mathematics;
using Touhou.Objects.Projectiles;

namespace Touhou.Graphics;

public class ProjectileHistoryDisplay : Rectangle {



    private static readonly Dictionary<Projectile.ProjectileType, string> spritesByType = new() {
        {Projectile.ProjectileType.Amulet, "amulet"},
        {Projectile.ProjectileType.LocalHoming, "spinningamulet"},
        {Projectile.ProjectileType.RemoteHoming, "spinningamulet"},
        {Projectile.ProjectileType.LocalTargetingAmuletGroup, "aimindicator"},
        {Projectile.ProjectileType.RemoteTargetingAmuletGroup, "aimindicator"},
        {Projectile.ProjectileType.TargetingAmulet, "amulet"},
        {Projectile.ProjectileType.SpecialAmulet, "amulet"},
        {Projectile.ProjectileType.YinYang, "yinyang"},
        {Projectile.ProjectileType.BombWave, "reimubombwave"},
    };
    private string name;
    private Queue<List<(uint, Projectile.ProjectileType, bool, bool, bool, Color4)>> history;

    public ProjectileHistoryDisplay(string name, Queue<List<(uint, Projectile.ProjectileType, bool, bool, bool, Color4)>> history) {

        this.name = name;
        this.history = history;
    }


    public override void Render() {
        //local

        var rectangle = new Rectangle() {

            Size = Size,
            FillColor = FillColor,
            StrokeColor = StrokeColor,
            StrokeWidth = StrokeWidth,

            Origin = Origin,
            Position = Position,
            Scale = Scale,
            Rotation = Rotation,
            IsUI = IsUI,
            Alignment = Alignment
        };

        rectangle.Render();

        var text = new Text() {
            DisplayedText = name,
            Origin = new Vector2(1f, 0.5f),
            Position = Position - Size * Origin + new Vector2(-20f, Size.Y * 0.5f),
            Color = Color4.White,
            CharacterSize = 40f,
            IsUI = true,
            Alignment = Alignment,
        };

        text.Render();

        float advance = 30f;

        foreach (var group in history) {

            float startingAdvance = advance;


            for (int i = 0; i < group.Count; i++) {

                (uint id, var type, bool isP1Owned, bool isPlayerOwned, bool isRemote, Color4 color) = group[i];

                float angle = MathF.Tau / 5f;

                var size = Game.Renderer.TextureAtlas.GetSize(spritesByType[type]);
                float height = size.X * MathF.Abs(MathF.Sin(angle)) + size.Y * MathF.Abs(MathF.Cos(angle));

                float alpha = 0.25f + (i + 1f) / group.Count * 0.75f;

                var sprite = new Sprite(spritesByType[type]) {
                    Origin = new Vector2(0.5f),

                    Position = Position - Size * Origin + new Vector2(advance, Size.Y * 0.5f),
                    Scale = new Vector2(Size.Y / height * 0.7f),
                    Rotation = angle,

                    Color = new Color4(color.R, color.G, color.B, alpha),
                    UseColorSwapping = true,
                    IsUI = IsUI,
                    Alignment = Alignment,
                };

                sprite.Render();

                advance += 10f;


            }

            uint firstId = group[0].Item1;

            var idText = new Text {
                DisplayedText = $"{firstId}",
                Origin = new Vector2(0f, 1f),
                Position = Position - Size * Origin + new Vector2(startingAdvance - 15f, Size.Y),
                Color = Color4.White,
                CharacterSize = 40f,
                IsUI = true,
                Alignment = Alignment,
            };

            var idTextOutline = new Text(idText);
            idTextOutline.Color = Color4.Black;
            idTextOutline.Boldness = 1f;



            idTextOutline.Render();
            // new Text(idTextOutline).Render();
            // new Text(idTextOutline).Render();
            // new Text(idTextOutline).Render();
            // new Text(idTextOutline).Render();
            idText.Render();

            advance += 40f;
        }
    }
}
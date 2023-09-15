using System.Text.Json.Serialization;
using OpenTK.Mathematics;

namespace Touhou.Graphics;

public class FontData {
    public uint TextureWidth { get; init; }
    public uint TextureHeight { get; init; }
    public uint SDFSize { get; init; }

    public float LineHeight { get; init; }
    public float Ascender { get; init; }
    public float Descender { get; init; }

    public Dictionary<int, Glyph> Glyphs { get; init; }
}

public struct Glyph {
    public int Unicode { get; init; }
    public float Advance { get; init; }
    public Box2 PlaneBounds { get; set; }
    public Box2 TextureBounds { get; init; }
}

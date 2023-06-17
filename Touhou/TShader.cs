using SFML.Graphics;
using SFML.Graphics.Glsl;
using SFML.System;

namespace Touhou;

public record TShader {

    public string Name { get; }
    private List<KeyValuePair<string, float>> floatUniforms = new();
    private List<KeyValuePair<string, Vec2>> vec2Uniforms = new();
    private List<KeyValuePair<string, Vec3>> vec3Uniforms = new();
    private List<KeyValuePair<string, Vec4>> vec4Uniforms = new();
    private List<KeyValuePair<string, bool>> boolUniforms = new();

    public TShader(string name) => Name = name;

    public void SetUniform(string name, float value) {
        floatUniforms.Add(new KeyValuePair<string, float>(name, value));
    }


    public void SetUniform(string name, Color value) {
        vec4Uniforms.Add(new KeyValuePair<string, Vec4>(name, new Vec4(value)));
    }

    public void SetUniform(string name, Vec2 value) {
        vec2Uniforms.Add(new KeyValuePair<string, Vec2>(name, value));
    }

    public void SetUniform(string name, Vec3 value) {
        vec3Uniforms.Add(new KeyValuePair<string, Vec3>(name, value));
    }

    public void SetUniform(string name, Vec4 value) {
        vec4Uniforms.Add(new KeyValuePair<string, Vec4>(name, value));
    }

    public void SetUniform(string name, bool value) {
        boolUniforms.Add(new KeyValuePair<string, bool>(name, value));
    }

    public void ApplyUniforms(Shader shader) {
        foreach (var (name, value) in floatUniforms) {
            shader.SetUniform(name, value);
        }

        foreach (var (name, value) in vec2Uniforms) {
            shader.SetUniform(name, value);
        }

        foreach (var (name, value) in vec3Uniforms) {
            shader.SetUniform(name, value);
        }

        foreach (var (name, value) in vec4Uniforms) {
            shader.SetUniform(name, value);
        }

        foreach (var (name, value) in boolUniforms) {
            shader.SetUniform(name, value);
        }
    }
}
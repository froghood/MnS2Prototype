using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Touhou.Graphics;

public class ShaderLibrary {


    private Dictionary<string, int> programs = new();

    private Dictionary<int, Dictionary<string, int>> shaderLocations = new();

    private int currentlyBoundProgram = 0;

    public void LoadShader(string shaderPath, ShaderType shaderType) {

        int shader = GL.CreateShader(shaderType);
        GL.ShaderSource(shader, File.ReadAllText(shaderPath));
        _ = CompileShader(shader);

        string name = Path.GetFileNameWithoutExtension(shaderPath);

        programs.TryAdd(name, GL.CreateProgram());

        int program = programs[name];
        GL.AttachShader(program, shader);

        GL.LinkProgram(program);

        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);
        if (status == 0) {
            string infoLog = GL.GetProgramInfoLog(program);
            Log.Error(infoLog);
        }

    }


    public void UseShader(string name) {
        if (programs.TryGetValue(name, out int program)) {
            if (program == currentlyBoundProgram) return;

            GL.UseProgram(program);
            currentlyBoundProgram = program;
        }
    }



    public void Uniform(string name, float value) { if (GetUniformLocation(name, out int location)) GL.Uniform1(location, value); }
    public void Uniform(string name, Vector2 value) { if (GetUniformLocation(name, out int location)) GL.Uniform2(location, value); }
    public void Uniform(string name, Vector3 value) { if (GetUniformLocation(name, out int location)) GL.Uniform3(location, value); }
    public void Uniform(string name, Vector4 value) { if (GetUniformLocation(name, out int location)) GL.Uniform4(location, value); }
    public void Uniform(string name, int value) { if (GetUniformLocation(name, out int location)) GL.Uniform1(location, value); }
    public void Uniform(string name, Vector2i value) { if (GetUniformLocation(name, out int location)) GL.Uniform2(location, value); }
    public void Uniform(string name, Vector3i value) { if (GetUniformLocation(name, out int location)) GL.Uniform3(location, value); }
    public void Uniform(string name, Vector4i value) { if (GetUniformLocation(name, out int location)) GL.Uniform4(location, value); }
    public void Uniform(string name, Color4 value) { if (GetUniformLocation(name, out int location)) GL.Uniform4(location, value); }
    public void Uniform(string name, bool value) { if (GetUniformLocation(name, out int location)) GL.Uniform1(location, Convert.ToInt32(value)); }
    public void Uniform(string name, Matrix2 value) { if (GetUniformLocation(name, out int location)) GL.UniformMatrix2(location, true, ref value); }
    public void Uniform(string name, Matrix4 value) { if (GetUniformLocation(name, out int location)) GL.UniformMatrix4(location, false, ref value); }

    private bool GetUniformLocation(string name, out int location) {

        if (shaderLocations.ContainsKey(currentlyBoundProgram)) {
            if (shaderLocations[currentlyBoundProgram].TryGetValue(name, out var _location)) {
                location = _location;
                return true;
            } else {
                location = GL.GetUniformLocation(currentlyBoundProgram, name);
                if (location >= 0) {
                    shaderLocations[currentlyBoundProgram][name] = location;
                    return true;
                } else {
                    return false;
                }
            }
        } else {
            location = GL.GetUniformLocation(currentlyBoundProgram, name);
            if (location >= 0) {
                shaderLocations[currentlyBoundProgram] = new Dictionary<string, int> {
                    {name, location},
                };
                return true;
            } else {
                return false;
            }
        }
    }

    private int CompileShader(int handle) {
        GL.CompileShader(handle);
        GL.GetShader(handle, ShaderParameter.CompileStatus, out int status);

        if (status == 0) {
            string infoLog = GL.GetShaderInfoLog(handle);
            //Log.Error($"\n{infoLog}");
        }

        return status;
    }


}
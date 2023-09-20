#version 400

out vec4 color;

uniform vec4 inColor;

void main() {
    color = vec4(inColor.rgb * inColor.a, inColor.a);
}
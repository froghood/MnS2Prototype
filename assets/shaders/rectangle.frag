#version 400

flat in vec4 vertexColor;

out vec4 color;




void main() {
    color = vec4(vertexColor.rgb * vertexColor.a, vertexColor.a);
}
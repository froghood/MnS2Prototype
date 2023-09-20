#version 400

layout(location = 0) in vec2 aPosition;

flat out vec4 vertexColor;

uniform vec4 fillColor;
uniform vec4 strokeColor;

void main() {
    
    vertexColor = gl_VertexID < 4 ? strokeColor : fillColor;
    
    gl_Position = vec4(aPosition, 0., 1.);
}


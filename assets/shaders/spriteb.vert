#version 400

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aUV;

uniform mat4 modelProjectionMatrix;
uniform vec2 alignment;

out vec2 vertexUV;

void main() {
    vertexUV = aUV;
    
    gl_Position = (modelProjectionMatrix * vec4(aPosition.xy, 0., 1.)) + vec4(alignment.xy, 0., 0.);
}


#version 400

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aUV;

uniform vec2 position;
uniform vec2 scale;
uniform mat2 rotation;

uniform bool isUI;
uniform vec2 uiAlignment;
uniform vec2 windowSize;
uniform vec2 cameraPosition;
uniform float cameraScale;

out vec2 vertexUV;

void main() {
    vertexUV = aUV;
    
    vec2 ndc = (aPosition * scale * rotation + position) / (windowSize * cameraScale) * 2. + (isUI ? uiAlignment : -cameraPosition);
 
    gl_Position = vec4(ndc, 0., 1.);
}


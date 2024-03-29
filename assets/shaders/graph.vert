#version 400

layout(location = 0) in vec2 aPosition;

uniform vec2 position;
uniform vec2 scale;
uniform mat2 rotation;

uniform bool isUI;
uniform vec2 uiAlignment;

uniform vec2 cameraPosition;
uniform vec2 cameraView;
uniform float windowAspectRatio;

float getCameraViewAspectRatio() {
    return cameraView.x / cameraView.y;
}

vec2 getCameraScale() {
    vec2 size;

    if (isUI) {
        size = vec2(2160. * windowAspectRatio, 2160.);
    } else {
        size = windowAspectRatio >= getCameraViewAspectRatio() ? vec2(cameraView.y * windowAspectRatio, cameraView.y) : vec2(cameraView.x, cameraView.x / windowAspectRatio);
    }

    return size;
}

void main() {
    
    
    
    gl_Position = vec4(((aPosition * rotation * scale + position) / getCameraScale() * 2.) + (isUI ? uiAlignment : -cameraPosition), 0., 1.);

    //gl_Position = vec4(aPosition, 0., 1.);
}


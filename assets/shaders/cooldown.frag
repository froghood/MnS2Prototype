
uniform sampler2D texture;

uniform vec2 resolution;
uniform vec2 position;
uniform vec2 size;

uniform float duration;
uniform bool disabled;

void main() {
    
    vec2 uv = (vec2(gl_FragCoord.x, resolution.y - gl_FragCoord.y) - position) / size;
    
    vec4 disabledColor = vec2(disabled ? 0.5 : 1., 1.).xxxy;
    
    
    if((atan(uv.x - 0.5, uv.y - 0.5) + radians(180)) / radians(360) > duration) {
        gl_FragColor = gl_Color * disabledColor;
    } else {
        
        gl_FragColor = gl_Color * vec2(0.7, 1.).xxxy * disabledColor;
    }
    
    
    
    // if (atan(coord.x, coord.y) / radians(180) < duration) {
    //     gl_FragColor = vec4(255,0,0,255);
    // } else {
    //     gl_FragColor = vec4(0,255,0,255);
    // }
    
}
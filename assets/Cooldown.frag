
uniform sampler2D texture;

uniform float duration;
uniform vec2 position;
uniform vec2 size;

void main() {
    
    vec4 textureColor = texture2D(texture, gl_TexCoord[0].xy);
    
    vec2 p = (gl_FragCoord.xy - position) / size;
    
    if((atan(p.x - 0.5, -(p.y - 0.5)) + radians(180)) / radians(360) > duration) {
        gl_FragColor = gl_Color;
    } else {
        
        gl_FragColor = gl_Color * vec2(0.7,1).xxxy;
    }
    
    
    
    // if (atan(coord.x, coord.y) / radians(180) < duration) {
    //     gl_FragColor = vec4(255,0,0,255);
    // } else {
    //     gl_FragColor = vec4(0,255,0,255);
    // }
    
}
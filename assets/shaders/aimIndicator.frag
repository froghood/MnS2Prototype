#version 120

uniform sampler2D texture;

uniform vec2 resolution;
uniform vec2 position;
uniform vec2 size;

uniform float angle;
uniform float arc;

float normalizeAngle(float a) {
    return mod(a + radians(180.), radians(360.)) - radians(180.);
}

void main()
{
    vec2 uv = (vec2(gl_FragCoord.x, resolution.y - gl_FragCoord.y) - position.xy) / size.xy;    

    float uvAngle = atan(uv.y - 0.5, uv.x - 0.5);
    
    float uvArc = abs(normalizeAngle(uvAngle - angle));
    
    vec4 tColor = texture2D(texture, gl_TexCoord[0].xy) * vec2(1., uvArc >= arc ? 0. : 1.).xxxy;
    

    gl_FragColor = gl_Color * tColor;
}
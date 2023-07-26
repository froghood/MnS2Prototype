#version 120

uniform sampler2D texture;

uniform vec2 resolution;
uniform vec2 position;
uniform vec2 size;

void main()
{
    vec2 fragCoord = vec2(gl_FragCoord.x, resolution.y - gl_FragCoord.y);
    
    vec2 pixel = (fragCoord.xy - position.xy) / size.xy;

    vec4 color = vec4(pixel.xy, 0., 1.);

    gl_FragColor = gl_Color * color;
}
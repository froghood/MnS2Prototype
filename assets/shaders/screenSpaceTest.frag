#version 120

uniform sampler2D texture;
uniform vec2 resolution;

void main()
{
    // lookup the pixel in the texture
    //vec4 pixel = texture2D(texture, gl_TexCoord[0].xy);

    vec4 pixel = vec4(gl_FragCoord.xy / resolution.xy, 0., 1.);

    // multiply it by the color
    gl_FragColor = gl_Color * pixel;
}
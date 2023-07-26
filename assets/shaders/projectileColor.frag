#version 120

uniform sampler2D texture;
uniform vec4 color;

void main()
{
    // lookup the pixel in the texture
    vec4 sourceColor = texture2D(texture, gl_TexCoord[0].xy);
  
    float darkness = sourceColor.r;
    float desaturation = sourceColor.g / sourceColor.r;
    
    vec4 newColor = vec4(color.rgb, sourceColor.a * color.a);
    
    newColor.rgb += (1. - color.rgb ) * desaturation;
    newColor.rgb *= darkness;

    // multiply it by the color
    gl_FragColor = gl_Color * newColor;
}
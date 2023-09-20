#version 400



uniform sampler2D texture0;

uniform float screenPxRange;
uniform vec4 textColor;
uniform float boldness;

in vec2 vertexUV;

out vec4 color;



float median(float r, float g, float b) {
    return max(min(r, g), min(max(r, g), b));
}

void main()
{    
    vec3 msd = texture(texture0, vertexUV).rgb;
    
    float scaledBoldness = clamp(boldness, 0., 1.) / 5;
    
    float signedDistance = median(msd.r, msd.g, msd.b) + scaledBoldness;

    
    
    
    float alpha = smoothstep(0.5 - screenPxRange, 0.5 + screenPxRange, signedDistance);

    
    color = vec4(textColor.rgb * textColor.a * alpha, textColor.a * alpha);

    
    //vec4 straightColor = mix(vec4(textColor.rgb, 0.), textColor, opacity);
    //color = vec4(straightColor.rgb * straightColor.a, straightColor.a);
}
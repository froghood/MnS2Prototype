#version 400

in vec2 vertexUV;

out vec4 color;

uniform sampler2D texture0;
uniform vec4 inColor;
uniform bool useColorSwapping;

void main() {
    
    vec4 textureColor = texture(texture0, vertexUV); 
   
    if (useColorSwapping) {
       
        float darkness = textureColor.r;
        float desaturation = textureColor.g / textureColor.r;
        
        vec4 newColor = vec4(inColor.rgb, textureColor.a * inColor.a);
        newColor.rgb += (1. - inColor.rgb) * desaturation;
        newColor.rgb *= darkness;
        
        color = vec4(newColor.rgb * newColor.a, newColor.a);
        
    } else {
        textureColor *= inColor;
        color = vec4(textureColor.rgb * textureColor.a, textureColor.a);
    }
    
}
#version 330 core
out vec4 FragColor;

in vec2 TexCoords;
uniform sampler2D texture1;


void main()
{    
    // gl_FragCoord.z is between 0.0 and 1.0
    FragColor = texture(texture1, TexCoords);
}
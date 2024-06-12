#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture1;

float near = 0.1;
float far = 100.0;

// non linear to linear mapping conversion
float LinearizeDepth(float depth) {
    float ndc = depth * 2.0 - 1.0;
    
    float eye = (2 * far * near) / (ndc*(far - near) - (far + near));
    float normalized = (-(eye) - near) / (far - near);

    return normalized;
}

void main()
{    
    // gl_FragCoord.z is between 0.0 and 1.0

    float depth = LinearizeDepth(gl_FragCoord.z);
    FragColor = vec4(vec3(depth), 1.0);
}
#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec3 aColor;
layout (location = 2) in vec2 offset;

out vec3 fColor;

uniform vec2 offsets[100];

void main()
{
    fColor = aColor;
    float instances = 100.0;
    float scale = gl_InstanceID / (instances - 1.0);
    vec2 pos = aPos * scale + offset;
    gl_Position = vec4(pos, 0.0, 1.0); 
}
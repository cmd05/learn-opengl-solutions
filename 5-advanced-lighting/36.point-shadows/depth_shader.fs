#version 330 core

in vec4 FragPos;

uniform vec3 lightPos;
uniform float far_plane;

void main() {
    float lightDistance = length(lightPos - FragPos.xyz);
    
    // map to [0,1] range
    lightDistance = lightDistance / far_plane;

    gl_FragDepth = lightDistance;
}
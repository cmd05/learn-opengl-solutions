#version 330 core

in vec4 FragPos;

uniform vec3 lightPos;
uniform float far_plane;

uniform mat4 camera_view;
uniform mat4 camera_projection;


void main() {
    // try discarding fragments outside camera view frustum
    vec4 camClipSpace = camera_projection * camera_view * FragPos;

    if(abs(camClipSpace.x) > camClipSpace.w || abs(camClipSpace.y) > camClipSpace.w || abs(camClipSpace.z) > camClipSpace.z)
        discard;

    float lightDistance = length(lightPos - FragPos.xyz);
    
    // map to [0,1] range
    lightDistance = lightDistance / far_plane;

    gl_FragDepth = lightDistance;
}
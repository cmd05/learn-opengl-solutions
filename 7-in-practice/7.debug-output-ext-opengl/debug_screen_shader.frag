#version 450 core

uniform samplerCube depthMap;

uniform vec3 lightPos;
uniform float far_plane;

out vec4 FragColor;

in VS_OUT {
    vec3 FragPos; // world space fragment position
    vec3 Normal;
    vec2 TexCoords;
} fs_in;

void main() {
    // get current depth
    vec3 lightToFrag = fs_in.FragPos.xyz - lightPos; 
    float currentDepth = length(lightToFrag);
    float closestDepth = texture(depthMap, lightToFrag)[0];
    closestDepth *= far_plane;

    float bias = 0.05;
    FragColor = vec4(vec3(currentDepth / far_plane), 1.0);

    if(currentDepth - bias > closestDepth)
        FragColor = vec4(vec3(0.1), 1.0); // shadow
}
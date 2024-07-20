#version 330 core
// mrt
layout (location = 0) out vec3 gPosition;
layout (location = 1) out vec3 gNormal;
layout (location = 2) out vec4 gAlbedoSpec;

in vec2 TexCoords;
in vec3 FragPos; // sent by vertex shader
in vec3 Normal;

uniform sampler2D texture_diffuse1;
uniform sampler2D texture_specular1;

// note all variables must be in the same coordinate space
// here we use the world coordinate space
void main() {
    gPosition = FragPos; // world space fragment position

    gNormal = normalize(Normal);

    gAlbedoSpec.rgb = texture(texture_diffuse1, TexCoords).rgb; // store diffuse per-fragment color
    gAlbedoSpec.a = texture(texture_specular1, TexCoords).r; // store the specular intensity in the alpha component of `gAlbedoSpec`
                                                             // specular map has only one component
}
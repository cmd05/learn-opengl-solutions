#version 330 core

out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
} fs_in;

struct Light {
    vec3 Position;
    vec3 Color;
};

uniform Light lights[16];
uniform sampler2D diffuseTexture;
uniform vec3 viewPos;

void main() {
    vec3 color = texture(diffuseTexture, fs_in.TexCoords).rgb; // samples from srgb texture
    vec3 normal = fs_in.Normal;

    // ambient
    vec3 ambient = 0.0 * color; // let ambient be 0
    // lighting
    vec3 lighting = vec3(0.0);

    // sum up diffuse components of all light sources
    for(int i = 0; i < 16; i++) {
        // diffuse
        vec3 lightDir = normalize(lights[i].Position - fs_in.FragPos);
        float diff = max(dot(lightDir, normal), 0.0);
        vec3 diffuse = lights[i].Color * diff * color;
        vec3 result = diffuse;

        // attenuation (use quadratic as we have gamma correction)
        float distance = length(fs_in.FragPos - lights[i].Position);
        result *= 1.0 / (distance * distance);
        lighting += result;
    }   

    // color values will not be clamped as we are using floating point color texture
    FragColor = vec4(ambient + lighting, 1.0);
}
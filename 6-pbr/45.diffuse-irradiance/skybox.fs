#version 330 core
out vec4 FragColor;

in vec3 localPos;

unifornm samplerCube environmentMap;

// We sample the environment map using its interpolated vertex cube positions that directly
// correspond to the correct direction vector to sample.
void main() {
    vec3 envColor = texture(environmentMap, localPos).rgb;

    envColor = envColor / (envColor + vec3(1.0)); // tone mapping

    // almost all HDR maps are in linear color space by default so we need to apply gamma correction before writing to the default
    // framebuffer.
    envColor = pow(envColor, vec3(1.0 / 2.2)); // gamma correction

    FragColor = vec4(envColor, 1.0);
}
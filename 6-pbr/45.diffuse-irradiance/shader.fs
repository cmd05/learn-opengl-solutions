#version 330 core
out vec4 FragColor;

in vec3 localPos; // interpolated localPos for fragment
                  // 3D sample vector

uniform sampler2D equirectangularMap;

const vec2 invAtan = vec2(0.1591, 0.3183);

// (spherical to cartesian)
// to sample equirectangular map as a cubemap
vec2 SampleSphericalMap(vec3 v) {
    vec2 uv = vec2(atan(v.z, v.x), asin(v.y));
    uv *= invAtan;
    uv *= 0.5;
    return uv;
}

void main() {
    vec2 uv = SampleSphericalMap(normalize(localPos));
    vec3 color = texture(equirectangularMap, uv).rgb;

    FragColor = vec4(color, 1.0);
}
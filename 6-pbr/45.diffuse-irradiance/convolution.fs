#version 330 core
out vec4 FragColor;
in vec3 localPos;

uniform samplerCube environmentMap; // hdr cubemap

const float PI = 3.14159265359;

void main() {
    // sample direction equals the hemisphere's orientation
    vec3 normal = normalize(localPos);

    vec3 irradiance = vec3(0.0);

    // convolution
    // implementation of the reimann sum
    // loop over the hemisphere for all w_i
    // sample the environment map in that direction w_i to get the radiance from that direction
    // here instead of w_i we have used polar and inclination angles

    vec3 up = vec3(0, 1, 0);
    vec3 right = cross(up, normal);
    up = cross(normal, right);

    float sampleDelta = 0.025;
    float nrSamples = 0.0;
    for(float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta) {
        for(float theta = 0.0; theta < 0.5 * PI; theta += sampleDelta) {
            // spherical to cartesian (in tangent space)
            vec3 tangentSample = vec3(sin(theta) * cos(phi), sin(theta) * sin(phi), cos(theta));

            // tangent space to world
            vec3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * N;

            // Note that we scale the sampled color value by cos(theta) due to
            // the light being weaker at larger angles and by sin(theta) to account for the smaller sample
            // areas in the higher hemisphere areas.
            irradiance += texture(environmentMap, sampleVec).rgb * cos(theta) * sin(theta);

            nrSamples++;
        }
    }

    irradiance = PI * irradiance  * (1.0 / float(nrSamples));

    FragColor = vec4(irradiance, 1.0);
}
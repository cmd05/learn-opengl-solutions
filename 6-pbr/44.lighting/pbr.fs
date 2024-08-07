#version 330 core

const float PI = 3.14159265359;

out vec4 FragColor;

// from vertex shader
in vec2 TexCoords;
in vec3 WorldPos;
in vec3 Normal;

uniform vec3 camPos; // viewPos

uniform vec3 albedo;
uniform float metallic;
uniform float roughness;
uniform float ao;

// lights
uniform vec3 lightPositions[4];
uniform vec3 lightColors[4];

const int nLights = 4;

/**
 * @param cosTheta dot product h.v
 * @param F0 surface reflection at zero incidence 
 */
vec3 fresnelSchlick(float cosTheta, vec3 F0) {
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

// normal distribution function
float DistributionGGX(vec3 N, vec3 H, float roughness) {
    float a = roughness * roughness; // we have taken alpha as the square of roughness
    float a2 = a * a;

    float NdotH2 = pow(max(dot(N, H), 0.0), 2.0);
    float denom = PI * pow((NdotH2) * (a2 - 1.0) + 1.0, 2);
    float num = a2;

    return num / denom;
}

// geometry function
float GeometrySchlickGGX(float NdotV, float roughness) {
    float a = roughness + 1.0;
    float k = (a * a) / 8.0;

    float num = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return num / denom;
}

// geometry smith function
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness) {
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);

    float ggx1 = GeometrySchlickGGX(NdotV, roughness);
    float ggx2 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

void main() {
    vec3 N = normalize(Normal);
    vec3 V = normalize(camPos - WorldPos); // fragment to view vector (same as W_o)

    // assume that most dielectric surfaces look visually correct with F0=0.04
    // for metallic surfaces, F0 (base reflectivity) will be given by the surface's albedo value
    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);
    
    // We can think of the loop as solving the integral over Ω for direct light sources, for all incoming light directions W_i.
    
    // point light sources contribute only to a single incoming light direction, 
    // so we only need to loop once per point light source

    // irradiance (outgoing radiance value)
    vec3 Lo = vec3(0.0); // Lo includes both diffuse and specular contributions

    for(int i = 0; i < nLights; i++) {
        // first do all lighting calculations in linear space
        vec3 L = normalize(lightPositions[i] - WorldPos); // frag to light vector (same as W_i)
        vec3 H = normalize(V + L); // halfway vector

        // messed up here, note use length(lightPositions[i] - WorldPos),
        // not length(L), since we need the vector from the entire length from frag to light source 
        float dist = length(lightPositions[i] - WorldPos);
        
        // use inverse square law instead of (constant-linear-quadratic attenuation equation)
        float attenuation = 1.0 / (dist * dist);
        float cosTheta = max(dot(N, L), 0.0); // both vectors are already normalized
        vec3 radiance = lightColors[i] * attenuation; // light color (radiance) when it reaches point p after effect of attenuation
        // vec3 radiance = lightColors[i] * attenuation * cosTheta;

        // calculate cook torrance brdf
        
        /// Specular contribution

        // NDC, G, F each give a value between 0 and 1
        float NDF = DistributionGGX(N, H, roughness); // normal distribution (larger the more microfacets aligned to H)
        float G = GeometrySmith(N, V, L, roughness); // geometry (smaller the more microfacets shadowed by other microfacets)
        vec3 F = fresnelSchlick(max(dot(H, V), 0.0), F0); // fresnel (proportion of specular reflectance)
                                                          // closer to 1, the more light and view vectors are at sheer angle to surface

        vec3 num = NDF * F * G; // fresnel component is a vec3
        float denom = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001; // + 0.0001 to prevent divide by zero
                                                                                // as view and light vector are at sheer angles,
                                                                                // higher specular highlight
    
        vec3 specular = num / denom; 

        // fresnel value directly corresponds to k_s (ratio of incoming light energy that gets reflected)
        vec3 kS = F; // specular contribution

        /// Diffuse Contribution
        
        // for energy conservation, the diffuse and specular light can't
        // be above 1.0 (unless the surface emits light); to preserve this
        // relationship the diffuse component (kD) should equal 1.0 - kS.
        vec3 kD = vec3(1.0) - kS; // portion that gets refracted

        kD *= 1.0 - metallic; // for metals, all refracted light gets directly absorbed without scattering (reflecting back)
                              // no diffuse effect of metallic surfaces
                              // so nullify the effect when surface is metallic

        // now implement reflectance equation to get: radiance due to reflection
        float NdotL = max(dot(N, L), 0.0);
        vec3 diffuse = kD * (albedo / PI);

        // diffuse component depends on both surface color (albedo) and light color (radiance)
        // specular component only depends on light color (radiance)

        // note that we already multiplied the BRDF by the Fresnel (kS) so we won't multiply by kS again
        Lo += (diffuse + specular) * radiance * NdotL; // add outgoing-radiance due to i-th light source
                                                       // more contribution of specular and diffuse components, when light vector
                                                       // and normal vector align more
    }

    // add ambient color to fragment
    vec3 ambient = vec3(0.03) * albedo * ao; // take ambient-occlusion into effect for ambient lighting

    vec3 color = ambient + Lo; // final color in hdr space

    color = color / (color + vec3(1.0)); // reinhard tone mapping
    color = pow(color, vec3(1.0 / 2.2)); // gamma correction

    FragColor = vec4(color, 1.0);
}
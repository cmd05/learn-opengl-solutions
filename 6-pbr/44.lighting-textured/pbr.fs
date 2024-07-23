#version 330 core

const float PI = 3.14159265359;

out vec4 FragColor;

// from vertex shader
in vec2 TexCoords;
in vec3 WorldPos;
in vec3 Normal;

uniform vec3 camPos; // viewPos

uniform sampler2D albedoMap;
uniform sampler2D normalMap;
uniform sampler2D metallicMap;
uniform sampler2D roughnessMap;
uniform sampler2D aoMap;

// lights
uniform vec3 lightPositions[4];
uniform vec3 lightColors[4];

const int nLights = 4;

// ----------------------------------------------------------------------------
// Easy trick to get tangent-normals to world-space to keep PBR code simplified.
// Don't worry if you don't get what's going on; you generally want to do normal 
// mapping the usual way for performance anyways; I do plan make a note of this 
// technique somewhere later in the normal mapping tutorial.
vec3 getNormalFromMap()
{
    vec3 tangentNormal = texture(normalMap, TexCoords).xyz * 2.0 - 1.0;

    vec3 Q1  = dFdx(WorldPos);
    vec3 Q2  = dFdy(WorldPos);
    vec2 st1 = dFdx(TexCoords);
    vec2 st2 = dFdy(TexCoords);

    vec3 N   = normalize(Normal);
    vec3 T  = normalize(Q1*st2.t - Q2*st1.t);
    vec3 B  = -normalize(cross(N, T));
    mat3 TBN = mat3(T, B, N);

    return normalize(TBN * tangentNormal);
}

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
    // Note that the albedo textures that come from artists are generally authored in sRGB space which
    // is why we first convert them to linear space before using albedo in our lighting calculations.
    vec3  albedo    = pow(texture(albedoMap, TexCoords).rgb, vec3(2.2)); // convert to linear space
    float metallic  = texture(metallicMap, TexCoords).r;
    float roughness = texture(roughnessMap, TexCoords).r;
    float ao        = texture(aoMap, TexCoords).r;

    vec3 N = getNormalFromMap();
    vec3 V = normalize(camPos - WorldPos); // fragment to view vector (same as W_o)

    // assume that most dielectric surfaces look visually correct with F0=0.04
    // for metallic surfaces, F0 (base reflectivity) will be given by the surface's albedo value
    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);
    
    // We can think of the loop as solving the integral over Ω for direct light sources.
    // loop over all light sources and add their outgoing radiance contributions
    
    // point light sources contribute only to a single incoming light direction, 
    // so we only need to loop once per light source 

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
        vec3 radiance = lightColors[i] * attenuation * cosTheta; // light color (radiance) when it reaches point p after effect of attenuation

        // calculate cook torrance brdf
        float NDF = DistributionGGX(N, H, roughness); // normal distribution
        float G = GeometrySmith(N, V, L, roughness); // geometry
        vec3 F = fresnelSchlick(max(dot(H, V), 0.0), F0); // fresnel

        vec3 num = NDF * F * G; // fresnel component is a vec3
        float denom = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001; // + 0.0001 to prevent divide by zero
    
        vec3 specular = num / denom; 

        // fresnel value directly corresponds to k_s (ratio of incoming light energy that gets reflected)
        vec3 kS = F; // specular contribution

        // for energy conservation, the diffuse and specular light can't
        // be above 1.0 (unless the surface emits light); to preserve this
        // relationship the diffuse component (kD) should equal 1.0 - kS.
        vec3 kD = vec3(1.0) - kS; // portion that gets refracted

        kD *= 1.0 - metallic; // metallic surfaces don't refract light in PBR, 
                              // so nullify the effect when surface is metallic

        // now implement reflectance equation to get: radiance due to reflection
        float NdotL = max(dot(N, L), 0.0);

        // note that we already multiplied the BRDF by the Fresnel (kS) so we won't multiply by kS again
        Lo += (kD * (albedo / PI) + specular) * radiance * NdotL; // add outgoing-radiance due to i-th light source
    }

    // add ambient color to fragment
    vec3 ambient = vec3(0.03) * albedo * ao; // take ambient-occlusion into effect

    vec3 color = ambient + Lo;

    color = color / (color + vec3(1.0)); // reinhard tone mapping
    color = pow(color, vec3(1.0 / 2.2)); // gamma correction

    FragColor = vec4(color, 1.0);
}
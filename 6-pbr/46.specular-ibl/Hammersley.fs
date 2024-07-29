float RadicalInverse_VdC(uint bits)
{
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}

// generate i-th sample (given that there will total N samples)
vec2 Hammersley(uint i, uint N)
{
    return vec2(float(i)/float(N), RadicalInverse_VdC(i));
}

const uint SAMPLE_COUNT = 4096u;
for(uint i = 0u; i < SAMPLE_COUNT; ++i)
{
    // in tangent space
    vec2 Xi = Hammersley(i, SAMPLE_COUNT); // i-th low discrepancy sequence sample vector
}

// without bit operations
// works on old hardware
// float VanDerCorpus(uint n, uint base)
// {
//     float invBase = 1.0 / float(base);
//     float denom = 1.0;
//     float result = 0.0;
    
//     for(uint i = 0u; i < 32u; ++i)
//     {
//         if(n > 0u)
//         {
//             denom = mod(float(n), 2.0);
//             result += denom * invBase;
//             invBase = invBase / 2.0;
//             n = uint(float(n) / 2.0);
//         }
//     }

//     return result;
// }

// // -----------------------------------------------------
// vec2 HammersleyNoBitOps(uint i, uint N)
// {
//     return vec2(float(i)/float(N), VanDerCorpus(i, 2u));
// }

// Importance Sampling:
// This gives us a sample vector *somewhat oriented around the expected microsurface's halfway
// vector* based on 
        // some input roughness and the 
        // low-discrepancy sequence value Xi.
vec3 ImportanceSampleGGX(vec2 Xi, vec3 N, float roughness)
{
    float a = roughness*roughness;
    float phi = 2.0 * PI * Xi.x;
    // get spherical angles corresponding to Xi
    float cosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a*a - 1.0) * Xi.y));
    float sinTheta = sqrt(1.0 - cosTheta*cosTheta);

    // from spherical coordinates to cartesian coordinates
    vec3 H;
    H.x = cos(phi) * sinTheta;
    H.y = sin(phi) * sinTheta;
    H.z = cosTheta;
    
    // from tangent-space vector to world-space sample vector
    vec3 up = abs(N.z) < 0.999 ? vec3(0.0, 0.0, 1.0) : vec3(1.0, 0.0, 0.0);
    
    vec3 tangent = normalize(cross(up, N));
    vec3 bitangent = cross(N, tangent);
    
    // bias 
    vec3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
    
    return normalize(sampleVec);
}


// -------------------------

#ver
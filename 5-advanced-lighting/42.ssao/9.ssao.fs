#version 330 core
out float FragColor;

in vec2 TexCoords;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D texNoise;

uniform vec3 samples[64];

// parameters (you'd probably want to use them as uniforms to more easily tweak the effect)
int kernelSize = 64;
float radius = 0.5;
float bias = 0.025;

// tile noise texture over screen based on screen dimensions divided by noise size
// makes rotation of kernels more rapid
const vec2 noiseScale = vec2(800.0/4.0, 600.0/4.0); 

uniform mat4 projection;

void main()
{
    // get input for SSAO algorithm
    vec3 fragPos = texture(gPosition, TexCoords).xyz;
    vec3 normal = normalize(texture(gNormal, TexCoords).rgb);
    vec3 randomVec = normalize(texture(texNoise, TexCoords * noiseScale).xyz); // get a random rotation vector from noise texture
    // note our random rotation vector has no z-component as defined in the render loop code

    // create TBN change-of-basis matrix: from tangent-space to view-space
    // note: randomVec is used to rotate the sample kernel in *tangent space*, therefore we defined its z-coordinate to be 0.0
    
    vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal)); // vector orthogonal to `normal` and in plane of `normal` and `randomVec`
    vec3 bitangent = cross(normal, tangent); 
    mat3 TBN = mat3(tangent, bitangent, normal);
    
    // iterate over the sample kernel and calculate occlusion factor
    float occlusion = 0.0; // occlusion lies between 0 and 1

    for(int i = 0; i < kernelSize; ++i)
    {
        // get sample position
        vec3 samplePos = TBN * samples[i]; // *sample vector* from tangent to view-space
        vec3 scaled_sample = samplePos * radius; // first scale the samplePos according to the radius we want
                                                  // remember in the render loop, samples were 
                                                  // defined in a hemisphere of radius of 1.0
        samplePos = fragPos + scaled_sample; // the addition to fragPos, actually gives the *position* of the sample point in view space
                                             // i.e wrt to our origin in view space
        
        // project sample position (to sample texture) (to get position on screen/texture)

        // get sample position in screen space 
        // (but the xyz values are transformed to [0,1] range instead of screen space resolution, so that we can sample the gbuffer textures)
        vec4 offset = vec4(samplePos, 1.0);
        offset = projection * offset; // from view to clip-space
        offset.xyz /= offset.w; // perspective divide
        // we could have added a redundant conversion to screen space resolutioon
        // and then to the texture range, but here we directly transform to the texture range
        offset.xyz = offset.xyz * 0.5 + 0.5; // transform to range 0.0 - 1.0
        
        // get depth of existing fragment at the current sample point from the gPosition texture
        // remember the sample point is just an imaginary point, it generally does not correspond to a real point
        float existingSampleDepth = texture(gPosition, offset.xy).z; // get depth value of the current sample in *view space*
        
        // range check & accumulate
        // as values exceed radius, they get closer to 0.0
        // when length is less than or equal to radius, it returns 1.0
        float rangeCheck = smoothstep(0.0, 1.0, radius / abs(fragPos.z - existingSampleDepth));
        
        // compare view space depths of `existingSampleDepth` and `samplePos`
        // since depth values are negative in view space (from -near to -far), the closer the value is to zero (i.e greater)
        // the closer it is to the screen

        // if existingSampleDepth is closer to the screen than the samplePos, the sample is getting occluded
        // bias to avoid acne effect
        occlusion += (existingSampleDepth >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck; // add 1 when occluded else 0
    }

    FragColor = 1.0 - (occlusion / kernelSize); // subtraction from one gives darker color when more occluded, for each fragment
    // FragColor = pow(1.0 - (occlusion / kernelSize), 64); // use power to tweak intensity of occlusion effect
}
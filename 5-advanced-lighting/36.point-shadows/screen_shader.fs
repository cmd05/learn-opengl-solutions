#version 330 core

uniform sampler2D diffuseTexture;
uniform samplerCube depthMap;

uniform vec3 lightPos;
uniform vec3 viewPos;

uniform vec3 lightColor;
uniform float far_plane;

out vec4 FragColor;

in VS_OUT {
    vec3 FragPos; // world space fragment position
    vec3 Normal;
    vec2 TexCoords;
} fs_in;

vec3 gridSamplingDisk[20] = vec3[]
(
   vec3(1, 1,  1), vec3( 1, -1,  1), vec3(-1, -1,  1), vec3(-1, 1,  1), 
   vec3(1, 1, -1), vec3( 1, -1, -1), vec3(-1, -1, -1), vec3(-1, 1, -1),
   vec3(1, 1,  0), vec3( 1, -1,  0), vec3(-1, -1,  0), vec3(-1, 1,  0),
   vec3(1, 0,  1), vec3(-1,  0,  1), vec3( 1,  0, -1), vec3(-1, 0, -1),
   vec3(0, 1,  1), vec3( 0, -1,  1), vec3( 0, -1, -1), vec3( 0, 1, -1)
);

float ShadowCalculation(vec3 fragPos) {
    // get current depth
    vec3 lightToFrag = fs_in.FragPos.xyz - lightPos; 
    float currentDepth = length(lightToFrag);

    float closestDepth = texture(depthMap, lightToFrag)[0];
    closestDepth *= far_plane;

    float bias = 0.15;
    // return currentDepth - bias > closestDepth ? 1.0 : 0.0; 
    
    float shadow = 0.0;
    int samples = 20;
    float viewDistance = length(viewPos - fragPos);
    float diskRadius = (1.0 + (viewDistance / far_plane)) / 25.0;
    
    for(int i = 0; i < samples; ++i)
    {
        float closestDepth = texture(depthMap, lightToFrag + gridSamplingDisk[i] * diskRadius).r;
        closestDepth *= far_plane;   // undo mapping [0;1]
        if(currentDepth - bias > closestDepth)
            shadow += 1.0;
    }
    
    shadow /= float(samples);

    return shadow;
}

void main() {
    vec3 color = texture(diffuseTexture, fs_in.TexCoords).rgb;
    vec3 normal = normalize(fs_in.Normal);
    
    // ambient
    float ambientStrength = 0.3;
    vec3 ambient = ambientStrength * lightColor;

    // diffuse
    vec3 lightDir = normalize(lightPos - fs_in.FragPos); // fragment to light direction
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * lightColor;

    // specular
    vec3 viewDir = normalize(viewPos - fs_in.FragPos); // fragment to incident-light (camera) direction
    vec3 reflectDir = reflect(-lightDir, normal); // from fragment towards reflection direction
    
    float spec = 0.0;
    vec3 halfwayDir = normalize(lightDir + viewDir);
    spec = pow(max(dot(halfwayDir, normal), 0.0), 64.0);

    vec3 specular = spec * lightColor;

    float shadow = ShadowCalculation(fs_in.FragPos);

    vec3 frag_col = (ambient + (1.0 - shadow) * (diffuse + specular)) * color;

    FragColor = vec4(frag_col, 1.0);
}
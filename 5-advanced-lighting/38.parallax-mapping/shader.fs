#version 330 core
out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec3 TangentFragPos;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec2 TexCoords;
} fs_in;

uniform sampler2D diffuseMap;
uniform sampler2D normalMap;
uniform sampler2D depthMap;

uniform float heightScale;

vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir) {
    // float height = texture(depthMap, texCoords)[0];
    // vec2 P = viewDir.xy * (height * heightScale); // ex: at A, P = 0.67 [height] * heightscale [0,1] * viewDir.xy [unit vector]
    // return texCoords - p;
    
    float height = texture(depthMap, texCoords).r;
    vec2 P = viewDir.xy / viewDir.z * heightScale;

    const float minLayers = 8.0;
    const float maxLayers = 32.0;
    // take the dot product of viewDir and +z-vector [top view] (both in tangent space)
    // take less samples when looking straight at a surface and more samples when looking at an angle
    float numLayers = mix(maxLayers, minLayers, max(dot(vec3(0.0, 0.0, 1.0), viewDir), 0.0));

    float layerDepth = 1 / numLayers;
    vec2 texOffset = P / numLayers;
    float currentLayerDepth = 0.0;

    // values at A (depthlayer = 0)
    vec2 currentTexCoords = texCoords;
    float currentDepthMapValue = texture(depthMap, currentTexCoords).r;
    
    // ex: (0, 0.67), (0.2, 1.0), (0.4, 0.73), (0.6, 0.38), (0.8, 0.0), (1.0, 0.05)
    // so stop at at layer=0.6

    while(currentLayerDepth < currentDepthMapValue) {
        currentTexCoords -= texOffset; // set tex coordinates to sample next layer
        currentLayerDepth += layerDepth; // set next layer depth value
        currentDepthMapValue = texture(depthMap, currentTexCoords).r; // sample next layer depth map value
    }

    // parallax occlusion mapping
    float afterDepth = currentDepthMapValue - currentLayerDepth;

    vec2 prevCoords = currentTexCoords + texOffset;
    float beforeDepth = texture(depthMap, prevCoords).r - (currentLayerDepth - layerDepth);
    float weight = afterDepth / (afterDepth - beforeDepth);

    // interpolated texture coordinates
    vec2 finalTexCoords = prevCoords * weight + currentTexCoords * (1.0 - weight);
    
    // Debugging
    // FragColor = vec4(vec3(length(P)), 1.0);

    return finalTexCoords;
}

void main() {
    vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
    vec2 texCoords = fs_in.TexCoords; // texture coordinates of Point A

    texCoords = ParallaxMapping(fs_in.TexCoords, viewDir); // texture coordinates of point P (close to B)
    if(texCoords.x > 1.0 || texCoords.y > 1.0 || texCoords.x < 0.0 || texCoords.y < 0.0) // discard fragment which go out of texture range
        discard;

    vec3 normal = texture(normalMap, texCoords).stp; // sample texture at new coordinates
    normal = normalize(normal * 2.0 - 1.0);

    vec3 color = texture(diffuseMap, texCoords).rgb; // sample color at new coordinates

    // ambient
    vec3 ambient = 0.1 * color;

    // diffuse
    vec3 lightDir = normalize(fs_in.TangentLightPos - fs_in.FragPos);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * color;

    // specular
    vec3 reflectDir = reflect(-lightDir, normal);
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);

    vec3 specular = vec3(0.2) * spec;

    FragColor = vec4(ambient + diffuse + specular, 1.0);
}
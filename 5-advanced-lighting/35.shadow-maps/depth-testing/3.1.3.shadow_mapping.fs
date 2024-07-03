#version 330 core
out vec4 FragColor;

in VS_OUT { // interpolated
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
    vec4 FragPosLightSpace;
} fs_in;

uniform sampler2D diffuseTexture; // wood texture
uniform sampler2D shadowMap;

uniform vec3 lightPos;
uniform vec3 viewPos;

float ShadowCalculation(vec4 fragPosLightSpace)
{
    // perform perspective divide (convert to NDC): [-1,-1] to [1,1]
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;

    // transform NDC to [0,1] range
    // as we want to sample from textures which have coordinates from (0,0,0) to (1,1,1)
    projCoords = projCoords * 0.5 + 0.5;

    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(shadowMap, projCoords.xy)[0];
    
    // get depth of current fragment from light's perspective
    // note: directional light can be thought of as a plane instead of a point
    float currentDepth = projCoords.z;

    // ----------- shadow biasing ---------------

   	// since our depth map has a limited resolution, when sampling the depth map using texture(shadowMap, projCoords.xy)[0],
	// multiple fragments can sample the same value, especially when they are away from the light source.
    
    //// due to limited precision we may also get different values of `projCoords.` and `closestDepth` causing
    // the shadow acne artifact
    
    // for example: for same fragment depth values should be equal, however due to limited resolution of depth map
    // and depth buffer of screen rendering we may get different values of `currentDepth` and `closestDepth` causing
    // shadow acne

    // float bias = 0.05;
    // return currentDepth - bias > closestDepth ? 1.0 : 0.0; // **bring the currentDepth a bit closer from the scene**
                                                           // so all the points which are actually lighted, do not show as shadow due to shadow acne
                                                           // then compare the values
                                                           // sometimes this may cause peter panning

   	// bias value is dependent on angle between light source and surface.
	// surfaces like the floor that are almost perpendicular to the 
	// light source get a small bias, while surfaces like the cube’s side-faces get a much larger bias.
    // calculate bias (based on depth map resolution and slope)
    vec3 normal = normalize(fs_in.Normal);
    vec3 lightDir = normalize(lightPos - fs_in.FragPos);
    float bias = max(0.05 * (1.0 - dot(normal, lightDir)), 0.005); // 0.005 to 0.05 - prevents peter panning

    // ------------------------------------------

    // PCF
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            vec2 offset = vec2(x, y) * texelSize;
            float pcfDepth = texture(shadowMap, projCoords.xy + offset)[0]; 
            shadow += currentDepth - bias > pcfDepth  ? 1.0 : 0.0; // find shadow of each texel with the bias
        }
    }
    shadow /= 9.0; // now shading value maybe floating point in [0,1] range
    
    // keep the shadow at 0.0 when outside the far_plane region of the light's orthogonal frustum.
    if(projCoords.z > 1.0)
        shadow = 0.0;
        
    return shadow;
}

void main()
{           
    vec3 color = texture(diffuseTexture, fs_in.TexCoords).rgb;
    vec3 normal = normalize(fs_in.Normal);
    vec3 lightColor = vec3(0.3);
    // ambient
    vec3 ambient = 0.3 * lightColor;
    // diffuse
    vec3 lightDir = normalize(lightPos - fs_in.FragPos);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * lightColor;
    // specular
    vec3 viewDir = normalize(viewPos - fs_in.FragPos);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = 0.0;
    vec3 halfwayDir = normalize(lightDir + viewDir);  
    spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
    vec3 specular = spec * lightColor;    

    // so far we have rendered the scene and colors as usual from the camera's perspective
	// now we must use the shadowMap (depthMap) to find where shadows will be

    // calculate shadow
    float shadow = ShadowCalculation(fs_in.FragPosLightSpace);                      
    vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * color;    
    
    FragColor = vec4(lighting, 1.0);
}
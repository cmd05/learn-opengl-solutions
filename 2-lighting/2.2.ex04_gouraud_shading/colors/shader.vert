#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;

out vec4 fragment_Color;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

// lighting uniforms
uniform vec3 lightPos;
uniform vec3 lightColor;
uniform vec3 objectColor;
uniform vec3 viewPos;

void main()
{
	gl_Position = projection * view * model * vec4(aPos, 1.0);

    // world position vector to vertex
	vec3 vertPos = vec3(model * vec4(aPos, 1.0));
    // normal matrix multiplication
	vec3 Normal = mat3(transpose(inverse(model))) * aNormal;

	// ambient
    // give the object a constant color due to scattered light
    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * lightColor;
  	
    // diffuse
    vec3 norm = normalize(Normal.xyz); // normal to fragment
    vec3 lightDir = normalize(lightPos - vertPos); // points to light source from fragment
    float diff = max(dot(norm, lightDir), 0.0); // dot product, gives a value in the range [0,1]
    vec3 diffuse = diff * lightColor; // diffuse value

    // specular
    vec3 viewDir = normalize(viewPos - vertPos); // points from fragment to the camera
    vec3 reflectDir = normalize(reflect(-lightDir, norm)); // reflect wrt normal vector
                                                           // points from fragment to direction of reflection
    
    // If we would set this to 1.0f we’d get a really bright specular component
    float specularStrength = 1.0;
    float shine_value = 32;
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shine_value); // value between [0,1] raised to the shine value
    vec3 specular = specularStrength * spec * lightColor; // specular value

    vec3 result = (ambient + diffuse + specular) * objectColor; // resultant color
    
    fragment_Color = vec4(result, 1.0);
}
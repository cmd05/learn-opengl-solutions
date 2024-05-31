#version 330 core
out vec4 FragColor;

in vec3 Normal;  
in vec3 FragPos;  

uniform vec3 lightPos;
uniform vec3 lightColor;
uniform vec3 objectColor;
uniform vec3 viewPos;

void main()
{
    // ambient
    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * lightColor;
  	
    // diffuse
    vec3 norm = normalize(Normal.xyz);
    vec3 lightDir = normalize(lightPos - FragPos); // points to light source
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    // specular
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm); // reflect wrt normal vector
    
    // If we would set this to 1.0f we�d get a really bright specular component
    float specularStrength = 0.5;
    float shine_value = 256;
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shine_value);
    vec3 specular = specularStrength * spec * lightColor;

    vec3 result = (ambient + diffuse + specular) * objectColor;
    FragColor = vec4(result, 1.0);
}
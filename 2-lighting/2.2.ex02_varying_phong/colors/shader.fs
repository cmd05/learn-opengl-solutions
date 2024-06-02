#version 330 core
out vec4 FragColor;

in vec3 FragPos; // this value is interpolated from world position of the vertexes of the triangle and 
                 // based on the position of the fragment in the triangle
in vec3 Normal; // normal is interpolated based on vertices and fragment position in triangle
                // barycentric interpolation of normals from the vertices works for our use case

// uniforms
uniform vec3 lightPos;
uniform vec3 lightColor;
uniform vec3 objectColor;
uniform vec3 viewPos;

uniform float ambientStrength;
uniform float specularStrength;
uniform float shineValue;
// uniform float diffVal;

void main()
{
    // ambient
    // give the object a constant color due to scattered light
    // float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * lightColor;
  	
    // diffuse
    vec3 norm = normalize(Normal.xyz); // normal to fragment
    vec3 lightDir = normalize(lightPos - FragPos); // points to light source from fragment
    float diffVal = max(dot(norm, lightDir), 0.0); // dot product, gives a value in the range [0,1]

    vec3 diffuse = diffVal * lightColor; // diffuse value

    // specular
    vec3 viewDir = normalize(viewPos - FragPos); // points from fragment to the camera
    vec3 reflectDir = normalize(reflect(-lightDir, norm)); // reflect wrt normal vector
                                                           // points from fragment to direction of reflection
    
    // If we would set this to 1.0f we’d get a really bright specular component
    // float specularStrength = 0.5;
    // float shineValue = 32;
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), shineValue); // value between [0,1] raised to the shine value
    vec3 specular = specularStrength * spec * lightColor; // specular value

    vec3 result = (ambient + diffuse + specular) * objectColor; // resultant color
    FragColor = vec4(result, 1.0);
}
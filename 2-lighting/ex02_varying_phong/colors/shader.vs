#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;

out vec3 FragPos;
out vec3 Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
	gl_Position = projection * view * model * vec4(aPos, 1.0);

	FragPos = vec3(model * vec4(aPos, 1.0)); // world position vector to vertex
											 // we don't need w-component
	
	// <vec4> Normal = model * vec4(aNormal, 0.0); // translation not required, hence w-component is set to zero

	Normal = mat3(transpose(inverse(model))) * aNormal; // normal matrix multiplication
														// to take care of non uniform scaling
														// ignore w-component by using vec3
}
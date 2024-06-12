#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D texture_diffuse1;
uniform vec4 gun_color;

void main() {
	FragColor = mix(texture(texture_diffuse1, TexCoords), gun_color, 0.1);
}
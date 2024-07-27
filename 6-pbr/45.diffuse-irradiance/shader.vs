// render a unit cube

#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 localPos; // 3D sample vector
                   // local position of vertex

uniform mat4 projection;
uniform mat4 view;

void main() {
    localPos = aPos;
    gl_Position = projection * view * vec4(localPos, 1.0);
}
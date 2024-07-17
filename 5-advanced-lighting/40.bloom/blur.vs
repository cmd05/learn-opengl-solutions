#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;

out vec2 TexCoords;

// we just have to blur the given texture
// so we don't need any transformations
void main() {
    TexCoords = aTexCoords;
    gl_Position = vec4(aPos, 1.0);
}
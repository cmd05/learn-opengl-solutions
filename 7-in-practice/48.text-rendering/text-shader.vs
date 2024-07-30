#version 330 core
layout (location = 0) in vec4 vertex; // <vec2 pos, vec2 texture_coords>
out vec2 TexCoords;

uniform mat4 projection;

void main() {
    gl_Position = projection * vec4(vertex.xy, 0.0, 1.0);
    TexCoords = vertex.zw; // last two components of input vertex
}
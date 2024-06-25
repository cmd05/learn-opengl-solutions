#version 330 core
layout (triangles) in;
layout (line_strip, max_vertices = 6) out;

float MAG = 0.2;

uniform mat4 projection;

in VS_OUT {
	vec3 normal;
} gs_in[];

void makeNormal(int index) {
	gl_Position = projection * gl_in[index].gl_Position;
	EmitVertex();
	gl_Position = projection * (gl_in[index].gl_Position + vec4(gs_in[index].normal, 0.0) * MAG);
	EmitVertex();

	EndPrimitive();
}
void main() {
	makeNormal(0);
	makeNormal(1);
	makeNormal(2);
};
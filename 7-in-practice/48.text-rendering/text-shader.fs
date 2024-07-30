#version 330 core
in vec2 TexCoords;
out vec4 color;

// mono-colored bitmap image of the glyph
uniform sampler2D text;
uniform vec3 textColor;

void main() {
    // By varying the output color’s alpha value, the
    // resulting pixel will be transparent for all the glyph's background colors and non-transparent for the
    // actual character pixels.
    vec4 sampled = vec4(1.0, 1.0, 1.0, texture(text, TexCoords).r);
    color = vec4(textColor, 1.0) * sampled;
}
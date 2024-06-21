#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D screenTexture;
uniform int processingMode;

const float offset = 1.0 / 300.0;

vec4 kernel_compute_color(sampler2D screen_texture, vec2 tex_coords, float kernel[9]) { // 3x3 kernel
    vec2 offsets[9] = vec2[](
        vec2(-offset, offset), // top-left
        vec2( 0.0f, offset), // top-center
        vec2( offset, offset), // top-right
        vec2(-offset, 0.0f), // center-left
        vec2( 0.0f, 0.0f), // center-center
        vec2( offset, 0.0f), // center-right
        vec2(-offset, -offset), // bottom-left
        vec2( 0.0f, -offset), // bottom-center
        vec2( offset, -offset) // bottom-right
    );

    vec3 sampleTex[9];

    for(int i = 0; i < 9; i++)
    {
        sampleTex[i] = vec3(texture(screen_texture, tex_coords.st + offsets[i]));
    }

    vec3 outputcol = vec3(0.0);
    for(int i = 0; i < 9; i++)
        outputcol += sampleTex[i] * kernel[i];

    return vec4(outputcol, 1.0);
}

void main()
{
    vec3 col = texture(screenTexture, TexCoords).rgb;

    if(processingMode == 0)
        FragColor = vec4(col, 1.0);
    if(processingMode == 1)
        FragColor = vec4(1.0 - col, 1.0);
    if(processingMode == 2) {
        float average = 0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b;
        FragColor = vec4(vec3(average), 1.0); // average of rgb components
    }
    if(processingMode == 3) {
        float kernel[9] = float[]( // sharp
            -1, -1, -1,
            -1, 9, -1,
            -1, -1, -1
        );

        FragColor = kernel_compute_color(screenTexture, TexCoords, kernel);
    }
    if(processingMode == 4) {
        float kernel[9] = float[](  // blur
            1.0 / 16, 2.0 / 16, 1.0 / 16,
            2.0 / 16, 4.0 / 16, 2.0 / 16,
            1.0 / 16, 2.0 / 16, 1.0 / 16
        );

        FragColor = kernel_compute_color(screenTexture, TexCoords, kernel);
    }
    if(processingMode == 5) {
        float kernel[9] = float[](  // edge detection
            1, 1, 1,
            1, -8, 1,
            1, 1, 1
        );

        FragColor = kernel_compute_color(screenTexture, TexCoords, kernel);
    }
}
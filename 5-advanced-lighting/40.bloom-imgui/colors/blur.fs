#version 330 core

out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D image;
uniform bool horizontal;
uniform int blur_distance;

uniform int blur_algorithm;

uniform float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216); // gaussian weights (note they are decreasing)

// kernel blur effect
vec4 kernel_compute_color(sampler2D screen_texture, vec2 tex_coords, float kernel[9]) { // 3x3 kernel
    const float offset = 1.0 / 300.0;
    // vec2 tex_offset = 1.0 / textureSize(image, 0); // size of single texel

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


vec4 box_blur(sampler2D screenTexture, vec2 tex_coords) {
    vec2 offset = 1.0 / textureSize(screenTexture, 0); // size of single texel
    vec4 color = vec4(0.0);

    int blur_rad = blur_distance;

    if(blur_distance % 2 == 0)
       blur_rad -= 1; // avoid even sized squares, so that this fragment is always centered

    int upper_bound = blur_rad / 2;
    int lower_bound = -upper_bound;

        
    for(int y = lower_bound; y <= upper_bound; y++) {
        for(int x = lower_bound; x <= upper_bound; x++) {
            vec2 coords = tex_coords + vec2(x * offset.x, y * offset.y);
            color += texture(screenTexture, coords);
        }
    }

    color /= (blur_rad * blur_rad);

    return color;
}

void main()
{
    if(blur_algorithm == 0) {
        // we sample the texture 9 times for each fragment
        // add a blur effect to the fragment color
        vec2 tex_offset = 1.0 / textureSize(image, 0); // size of single texel
        vec3 result = texture(image, TexCoords).rgb * weight[0]; // sample this fragment

        if(horizontal)
        {
            // sample 4 pixels to left and 4 pixels to the right
            // add their weighted color to our final fragment color
            // NOTE: the pingpong buffer textures use CLAMP_TO_EDGE, so we don't need to worry about sampling outside texture range
            for(int i = 1; i < blur_distance; ++i)
            {
                result += texture(image, TexCoords + vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
                result += texture(image, TexCoords - vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
            }
        }
        else
        {
            // sample 4 pixels to the top and 4 pixels to the bottom
            // add their weighted color to our final fragment color

            for(int i = 1; i < blur_distance; ++i)
            {
                result += texture(image, TexCoords + vec2(0.0, tex_offset.y * i)).rgb * weight[i];
                result += texture(image, TexCoords - vec2(0.0, tex_offset.y * i)).rgb * weight[i];
            }
        }

        FragColor = vec4(result, 1.0);
    } else if(blur_algorithm == 1) {
        float kernel[9] = float[](  // blur
            1.0 / 16, 2.0 / 16, 1.0 / 16,
            2.0 / 16, 4.0 / 16, 2.0 / 16,
            1.0 / 16, 2.0 / 16, 1.0 / 16
        );

        FragColor = kernel_compute_color(image, TexCoords, kernel);
    } else if(blur_algorithm == 2) {
        FragColor = box_blur(image, TexCoords);
    }
}
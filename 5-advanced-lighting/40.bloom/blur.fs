#version 330 core

out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D image;
uniform bool horizontal;
uniform float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216); // gaussian weights (note they are decreasing)

void main()
{
    // we sample the texture 9 times for each fragment
    // add a blur effect to the fragment color

    vec2 tex_offset = 1.0 / textureSize(image, 0); // size of single texel
    vec3 result = texture(image, TexCoords).rgb * weight[0]; // sample this fragment
    if(horizontal)
    {
        // sample 4 pixels to left and 4 pixels to the right
        // add their weighted color to our final fragment color
        // NOTE: the pingpong buffer textures use CLAMP_TO_EDGE, so we don't need to worry about sampling outside texture range
        for(int i = 1; i < 5; ++i)
        {
            result += texture(image, TexCoords + vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
            result += texture(image, TexCoords - vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
        }
    }
    else
    {
        // sample 4 pixels to the top and 4 pixels to the bottom
        // add their weighted color to our final fragment color

        for(int i = 1; i < 5; ++i)
        {
            result += texture(image, TexCoords + vec2(0.0, tex_offset.y * i)).rgb * weight[i];
            result += texture(image, TexCoords - vec2(0.0, tex_offset.y * i)).rgb * weight[i];
        }
    }

    FragColor = vec4(result, 1.0);
}
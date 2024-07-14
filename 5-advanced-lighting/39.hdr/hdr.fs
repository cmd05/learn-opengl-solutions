#version 330 core

out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D hdrBuffer; // floating point color texture
uniform bool hdr;
uniform float exposure;

float map(float value, float min1, float max1, float min2, float max2) {
    return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
}

float lg(float n, float b) {
    return log(n) / log(b);
}

void main() {
    const float gamma = 2.2;
    vec3 hdrColor = texture(hdrBuffer, TexCoords).rgb;

    if(hdr) {
        // reinhard tone mapping
        
        /// 1. without using exposure parameter
        // dark details are less visible, while bright details are more visible

        // const float gamma = 2.2;
        // vec3 mapped = hdrColor / (hdrColor + vec3(1.0));
        // // gamma correction
        // mapped = pow(mapped, vec3(1.0 / gamma));

        // FragColor = vec4(mapped, vec3(1.0 / gamma));

        /// 2. using exposure parameter

        // float exposure_val = 2.0;
        // float limit_col = 3.27;
        // if(hdrColor.r > limit_col || hdrColor.g > limit_col || hdrColor.b > limit_col)
        //     exposure_val = 0.;
        // if(hdrColor.r < exposure || hdrColor.g < exposure || hdrColor.b < exposure)
        //     exposure_val = 100.0;
        
        // if(luminance > exposure)
        //     exposure_val = 0.0;

        /// try some stuff
        float base = exposure;
        float luminance = (0.2126*hdrColor.r + 0.7152*hdrColor.g + 0.0722*hdrColor.b);
        float lum_limit = exposure;
        if(luminance > lum_limit) // ignore too bright spots
            luminance = lum_limit;
        
        float exposure_val = map(luminance, 0.015, lum_limit, 3.5, 0.2);
        // float exposure_val = pow(base, map(luminance, 0.015, lum_limit, lg(3.5, base), lg(0.2, base)));

        const float gamma = 2.2;
        
        // exposure tone mapping
        vec3 mapped = vec3(1.0) - vec3(exp(-hdrColor * exposure_val));
        // gamma correction
        mapped = pow(mapped, vec3(1.0 / gamma));

        FragColor = vec4(mapped, 1.0);

    } else {
        // simply pass through value
        // gets clamped between 0.0 and 1.0
        FragColor = vec4(hdrColor, 1.0);
    }
}
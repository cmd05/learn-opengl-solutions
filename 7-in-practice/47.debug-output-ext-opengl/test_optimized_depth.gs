#version 450 core
layout (triangles) in;
layout (triangle_strip, max_vertices = 18) out;

uniform mat4 camera_view;
uniform mat4 camera_projection;

// order: right, left, top, bottom, near and far.
uniform mat4 shadowMatrices[6];

out vec4 FragPos;

vec3 right_plane[4] = {
            // right face
            vec3(1.0, -1.0, -1.0), // bottom-right
            vec3( 1.0,  1.0, -1.0), // top-right         
            vec3( 1.0,  1.0,  1.0),// top-left
            vec3(1.0, -1.0,  1.0),// bottom-left     
};

vec3 left_plane[4] = {
            // left face
            vec3(-1.0,  1.0, -1.0), // top-left
            vec3(-1.0, -1.0, -1.0), // bottom-left
            vec3(-1.0, -1.0,  1.0), // bottom-right
            vec3(-1.0,  1.0,  1.0), // top-right
};

vec3 top_plane[4] = {
            // top face
             vec3(1.0,  1.0 , 1.0), // bottom-right
             vec3(1.0,  1.0, -1.0), // top-right     
            vec3(-1.0,  1.0, -1.0), // top-left
            vec3(-1.0,  1.0,  1.0),   // bottom-left        
};

vec3 bottom_plane[4] = {
            // bottom face
             vec3(1.0, -1.0, -1.0), // top-left
             vec3(1.0, -1.0,  1.0), // bottom-left
            vec3(-1.0, -1.0,  1.0), // bottom-right
            vec3(-1.0, -1.0, -1.0), // top-right
};

vec3 front_plane[4] = {
            // front face
            vec3(-1.0, -1.0,  1.0), // bottom-left
             vec3(1.0, -1.0,  1.0),// bottom-right
             vec3(1.0,  1.0,  1.0), // top-right
            vec3(-1.0,  1.0,  1.0),// top-left
};

vec3 back_plane[4] = {
            // back face
            vec3(-1.0, -1.0, -1.0), // bottom-left
             vec3(1.0,  1.0, -1.0), // top-right
             vec3(1.0, -1.0, -1.0), // bottom-right         
            vec3(-1.0,  1.0, -1.0), // top-left
};

// Function to check if a point is inside the camera's frustum
bool IsPointInsideFrustum(vec4 point) {
    // Transform point to clip space
    vec4 clipSpacePos = camera_projection * camera_view * point;

    // Perform perspective division
    vec3 ndcSpacePos = clipSpacePos.xyz / clipSpacePos.w;

    // Check if the point is within the normalized device coordinates
    return all(lessThanEqual(abs(ndcSpacePos), vec3(1.0))) && (clipSpacePos.w > 0.0);
}

// Function to test if a quad is inside the camera frustum
bool IsQuadInFrustum(vec4 v0, vec4 v1, vec4 v2, vec4 v3) {
    // Check each vertex of the quad
    bool v0Inside = IsPointInsideFrustum(v0);
    bool v1Inside = IsPointInsideFrustum(v1);
    bool v2Inside = IsPointInsideFrustum(v2);
    bool v3Inside = IsPointInsideFrustum(v3);

    // If any vertex is inside the frustum, consider the quad inside
    if (v0Inside || v1Inside || v2Inside || v3Inside) {
        return true;
    }

    // Additional check: if all vertices are outside one side of the frustum
    // Transform vertices to clip space for all frustum plane checks
    vec4 clipV0 = camera_projection * camera_view * v0;
    vec4 clipV1 = camera_projection * camera_view * v1;
    vec4 clipV2 = camera_projection * camera_view * v2;
    vec4 clipV3 = camera_projection * camera_view * v3;

    // Define frustum planes (normals pointing inwards in clip space)
    vec4 frustumPlanes[6] = {
        vec4(1.0, 0.0, 0.0, 1.0),  // Right
        vec4(-1.0, 0.0, 0.0, 1.0), // Left
        vec4(0.0, 1.0, 0.0, 1.0),  // Top
        vec4(0.0, -1.0, 0.0, 1.0), // Bottom
        vec4(0.0, 0.0, 1.0, 1.0),  // Near
        vec4(0.0, 0.0, -1.0, 1.0)  // Far
    };

    // Check each plane
    for (int i = 0; i < 6; ++i) {
        vec4 plane = frustumPlanes[i];
        bool v0Outside = dot(clipV0, plane) < 0.0;
        bool v1Outside = dot(clipV1, plane) < 0.0;
        bool v2Outside = dot(clipV2, plane) < 0.0;
        bool v3Outside = dot(clipV3, plane) < 0.0;

        // If all vertices are outside one plane, the quad is outside
        if (v0Outside && v1Outside && v2Outside && v3Outside) {
            return false;
        }
    }

    // Quad intersects frustum
    return true;
}

bool checkPlane(int i) {
    if(i == 0)
        return IsQuadInFrustum(vec4(right_plane[0], 1.0), vec4(right_plane[1], 1.0), vec4(right_plane[2], 1.0), vec4(right_plane[3], 1.0));
    if(i == 1)
        return IsQuadInFrustum(vec4(left_plane[0], 1.0), vec4(left_plane[1], 1.0), vec4(left_plane[2], 1.0), vec4(left_plane[3], 1.0));
    if(i == 2)
        return IsQuadInFrustum(vec4(top_plane[0], 1.0), vec4(top_plane[1], 1.0), vec4(top_plane[2], 1.0), vec4(top_plane[3], 1.0));
    if(i == 3)
        return IsQuadInFrustum(vec4(bottom_plane[0], 1.0), vec4(bottom_plane[1], 1.0), vec4(bottom_plane[2], 1.0), vec4(bottom_plane[3], 1.0));
    if(i == 4)
        return IsQuadInFrustum(vec4(front_plane[0], 1.0), vec4(front_plane[1], 1.0), vec4(front_plane[2], 1.0), vec4(front_plane[3], 1.0));
    if(i == 5)
        return IsQuadInFrustum(vec4(back_plane[0], 1.0), vec4(back_plane[1], 1.0), vec4(back_plane[2], 1.0), vec4(back_plane[3], 1.0));
}

void main() {
    for(int face = 0; face < 6; face++) {
        if(!checkPlane(face))
            continue;
        
        gl_Layer = face;

        for(int i = 0; i < 3; i++) {
            FragPos = gl_in[i].gl_Position; // world space position
            gl_Position = camera_projection * camera_view * FragPos;
            gl_Position = shadowMatrices[face] * FragPos; // light clip space position
            EmitVertex();
        }
        EndPrimitive();
    }
}
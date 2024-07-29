// vertex shader
#version 330 core
layout (location = 0) in vec2 position;
layout (location = 1) in vec2 texCoords;
out vec2 TexCoords;

void main()
{
    gl_Position = vec4(position, 0.0f, 1.0f);
    TexCoords = texCoords;
}

// fragment shader
#version 330 core
out vec4 FragColor;
in vec2 TexCoords;
uniform sampler2D fboAttachment;

void main()
{
    FragColor = texture(fboAttachment, TexCoords);
}

// main.cpp

void DisplayFramebufferTexture(unsigned int textureID)
{
    if (!Initialized)
    {
        // initialize shader and vao w/ NDC vertex coordinates
        // at top-right of the screen
        [...]
    }
    
    glActiveTexture(GL_TEXTURE0);
    glUseProgram(shaderDisplayFBOOutput);
    glBindTexture(GL_TEXTURE_2D, textureID);
    glBindVertexArray(vaoDebugTexturedRect);
    glDrawArrays(GL_TRIANGLES, 0, 6);
    glBindVertexArray(0);
    glUseProgram(0);
}

int main()
{
    [...]
    while (!glfwWindowShouldClose(window))
    {
        [...]
        DisplayFramebufferTexture(fboAttachment0);
        glfwSwapBuffers(window);
    }
}
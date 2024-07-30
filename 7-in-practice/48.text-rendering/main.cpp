#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <stb/stb_image.h>

#include <learnopengl/shader.h>
#include <learnopengl/camera.h>
#include <learnopengl/model.h>

#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>

#include "ft2build.h"
#include <freetype/freetype.h>
#include <freetype/ftoutln.h>
#include FT_FREETYPE_H

#include <iostream>

void framebuffer_size_callback(GLFWwindow* window, int width, int height);
void mouse_callback(GLFWwindow* window, double xpos, double ypos);
void scroll_callback(GLFWwindow* window, double xoffset, double yoffset);
void processInput(GLFWwindow* window);
unsigned int loadTexture(const char* path);

void renderQuad();

int SCR_WIDTH = 800;
int SCR_HEIGHT = 600;

Camera camera(glm::vec3(0, 0, 3));
float lastX = (float) SCR_WIDTH / 2.0;
float lastY = (float) SCR_HEIGHT / 2.0;

bool firstMouse = true;

float deltaTime = 0.0f;
float lastFrame = 0.0f;

struct Character {
    unsigned int TextureID; // id handle of the glyph texture
    glm::ivec2 Size; // size of glpyh
    glm::ivec2 Bearing; // offset from baseline to left and top
    unsigned int Advance; // offset to advance to next glyph
};

std::map<char, Character> Characters;

void RenderText(Shader& s, std::string text, float x, float y, float scale, glm::vec3 color, unsigned int textVAO, unsigned int textVBO) {
    // activate corresponding render state
    s.use();
    glUniform3f(glGetUniformLocation(s.ID, "textColor"), color.x, color.y, color.z);
    glActiveTexture(GL_TEXTURE0);
    glBindVertexArray(textVAO);

    // iterate through all characters
    std::string::const_iterator c;
    for(c = text.begin(); c != text.end(); c++) {
        Character ch = Characters[*c];

        /// bottom left coordinates of the quad
        
        float xpos = x + ch.Bearing.x * scale;

        // Some characters (like 'p' or 'g') are rendered slightly below the baseline, so the quad should
        // also be positioned slightly below RenderText's y value.
        //  y - (...) so that it is below the baseline for characters like 'p'
        float ypos = y - (ch.Size.y - ch.Bearing.y) * scale;

        float w = ch.Size.x * scale;
        float h = ch.Size.y * scale;

        // update VBO for each character
        float vertices[6][4] = {
            { xpos, ypos + h, 0.0f, 0.0f },
            { xpos, ypos, 0.0f, 1.0f },
            { xpos + w, ypos, 1.0f, 1.0f },
            { xpos, ypos + h, 0.0f, 0.0f },
            { xpos + w, ypos, 1.0f, 1.0f },
            { xpos + w, ypos + h, 1.0f, 0.0f }
        };

        // render glyph texture over quad
        glBindTexture(GL_TEXTURE_2D, ch.TextureID);
        // update content of VBO memory
        glBindBuffer(GL_ARRAY_BUFFER, textVBO);
        glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(vertices), vertices);
        glBindBuffer(GL_ARRAY_BUFFER, 0);
        // render quad
        glDrawArrays(GL_TRIANGLES, 0, 6);
        // advance cursors for next glyph (advance is 1/64 pixels)
        x += (ch.Advance >> 6) * scale; // bitshift by 6 (2^6 = 64)
    }

    glBindVertexArray(0);
    glBindTexture(GL_TEXTURE_2D, 0);
}

int main() {
    // initialize glfw
    glfwInit();
 
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

#ifdef __APPLE__
    glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
#endif

    // glfw window creation
    GLFWwindow* window = glfwCreateWindow(SCR_WIDTH, SCR_HEIGHT, "practice", NULL, NULL);
    if(window == NULL) {
        std::cout << "failed to create glfw window" << std::endl;
        return -1;
    }

    // set window callbacks
    glfwMakeContextCurrent(window);
    glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);
    glfwSetCursorPosCallback(window, mouse_callback);
    glfwSetScrollCallback(window, scroll_callback);

    glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);

    // load glad
    if(!gladLoadGLLoader((GLADloadproc) glfwGetProcAddress)) {
        std::cout << "failed to initialize glad" << std::endl;
        return -1;
    }

    // freetype initialization
    FT_Library ft;
    if (FT_Init_FreeType(&ft))
        std::cout << "ERROR::FREETYPE: Could not init FreeType Library" << std::endl;
    
    FT_Face face;
    if (FT_New_Face(ft, "../../resources/fonts/Antonio-Bold.ttf", 0, &face))
        std::cout << "ERROR::FREETYPE: Failed to load font" << std::endl;

    FT_Set_Pixel_Sizes(face, 0, 48);

    if (int x = FT_Load_Char(face, 'x', FT_LOAD_RENDER))
       std::cout << "ERROR::FREETYPE: Failed to load Glyph" << std::endl;
    
    // OpenGL requires that textures all have a 4-byte alignment e.g. their size is always a multiple of
    // 4 bytes. Normally this won't be a problem since most textures have a width that is a multiple of 4
    // and/or use 4 bytes per pixel, but since we now only use a single byte per pixel, the texture can have
    // any possible width. By setting its unpack alignment to 1 we ensure there are no alignment issues
    // (which could cause segmentation faults).
    glPixelStorei(GL_UNPACK_ALIGNMENT, 1); // no byte alignment restriction

    for(unsigned char c = 0; c < 128; c++) {
        // load character glyph
        if(FT_Load_Char(face, c, FT_LOAD_RENDER)) {
            std::cout << "ERROR::FREETYPE: Failed to load glyph" << std::endl;
            continue;
        }

        // generate texture
        unsigned int texture;
        glGenTextures(1, &texture);
        glBindTexture(GL_TEXTURE_2D, texture);
        // use GL_RED as:
        // The bitmap generated from the glyph is a grayscale 8-bit image where each color is represented by a single byte.
        // We accomplish this by creating a texture where each byte corresponds to the texture color's red component (first byte of its color vector).
        glTexImage2D(
            GL_TEXTURE_2D,
            0,
            GL_RED,
            face->glyph->bitmap.width,
            face->glyph->bitmap.rows,
            0,
            GL_RED,
            GL_UNSIGNED_BYTE,
            face->glyph->bitmap.buffer
        );

        // set texture options
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

        // now store character for later use
        Character character = {
            texture,
            glm::ivec2(face->glyph->bitmap.width, face->glyph->bitmap.rows),
            glm::ivec2(face->glyph->bitmap_left, face->glyph->bitmap_top),
            static_cast<unsigned int>(face->glyph->advance.x)
        };

        Characters.insert(std::pair<char, Character>(c, character));
    }

    // configure opengl global state
    glEnable(GL_DEPTH_TEST);
    glEnable(GL_BLEND); // for blending in text-shader.fs
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

    // shader program
    Shader shader{"shader.vs", "shader.fs"};
    Shader textShader{"text-shader.vs", "text-shader.fs"};

    // textures
    stbi_set_flip_vertically_on_load(true); // tell stb_image.h to flip loaded texture's on the y-axis.
    unsigned int woodTexture = loadTexture("../../resources/textures/sigma-what-the-sigma.png");
    
    // draw a rectangle on the screen with EBO
    const float vertices[] = {
        // pos          // tex coords
        -0.5, -0.5, 0,  0, 0,  // bottom left
        0.5, -0.5, 0,   1, 0, // bottom right
        0.5, 0.5, 0,    1, 1, // top right
        -0.5, 0.5, 0,    0, 1, // top left
    };

    const int indices[] = {
        0, 1, 3,
        3, 2, 1
    };

    unsigned int VAO, VBO, EBO;
    glGenVertexArrays(1, &VAO);
    glGenBuffers(1, &VBO);
    glGenBuffers(1, &EBO);

    glBindVertexArray(VAO);
    glBindBuffer(GL_ARRAY_BUFFER, VBO);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO);

    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);

    glEnableVertexAttribArray(0);
    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 5 * sizeof(float), (void*) 0);

    glEnableVertexAttribArray(1);
    glVertexAttribPointer(1, 2, GL_FLOAT, GL_FALSE, 5 * sizeof(GL_FLOAT), (void*) (3 * sizeof(float)));

    glBindVertexArray(0);

    unsigned int textVAO, textVBO;
    glGenVertexArrays(1, &textVAO);
    glGenBuffers(1, &textVBO);
    glBindVertexArray(textVAO);

    glBindBuffer(GL_ARRAY_BUFFER, textVBO);
    // GL_DYNAMIC_DRAW: since we will be updating the VBO's memory quite often
    glBufferData(GL_ARRAY_BUFFER, sizeof(float) * 6*4, NULL, GL_DYNAMIC_DRAW); // 6 input vertices with 4 floats each

    glEnableVertexAttribArray(0);
    glVertexAttribPointer(0, 4, GL_FLOAT, GL_FALSE, 4 * sizeof(float), 0);

    glBindBuffer(GL_ARRAY_BUFFER, 0);
    glBindVertexArray(0);

    shader.use();
    shader.setInt("woodTexture", 0);

    // text color
    float r=1.0;
    float g=0.0;
    float b=0.0;

    float lastColorUpdate = 0.0;
    while(!glfwWindowShouldClose(window)){
        float currentFrame = (float) (glfwGetTime());
        deltaTime = currentFrame - lastFrame;
        lastFrame = currentFrame;

        processInput(window);

        glClearColor(0.1, 0.1, 0.1, 1.0);
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        shader.use();

        glm::mat4 model, view, projection;
        model = view = projection = glm::mat4(1.0);
        
        model = glm::scale(model, glm::vec3(1.0 + abs(sin((float) glfwGetTime())) / 2.0, 1.0 + abs(sin((float) glfwGetTime())) / 2.0, 1));
        view = camera.GetViewMatrix();
        projection = glm::perspective(glm::radians(camera.Zoom), (float) SCR_WIDTH / SCR_HEIGHT, 0.1f, 100.0f);

        // shader.setMat4("model", model);
        // shader.setMat4("view", view);
        // shader.setMat4("projection", projection);
        shader.setMat4("scaleMatrix", model);

        glActiveTexture(GL_TEXTURE0);
        glBindTexture(GL_TEXTURE_2D, woodTexture);
        glBindVertexArray(VAO);

        // glDrawArrays(GL_TRIANGLES, 0, 36);
        glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, 0);

        textShader.use();
        // We set the projection matrix's bottom parameter to 0.0f and its top parameter equal to the
        // window's height. The result is that we specify coordinates with y values ranging from the bottom
        // part of the screen (0.0f) to the top part of the screen (600.0f). This means that the point (0.0,
        // 0.0) now corresponds to the bottom-left corner.
        projection = glm::ortho(0.0f, 800.0f, 0.0f, 600.0f);
        textShader.setMat4("projection", projection);

        float deltaUpdate = currentFrame - lastColorUpdate;
        if(deltaUpdate > 0.005) {
            if(r > 0.1 && b < 0.1){
                r-=0.001;
                g+=0.001;
            }
            if(g > 0.1 && r < 0.1){
                g-=0.001;
                b+=0.001;
            }
            if(b > 0.1 && g < 0.1){
                r+=0.001;
                b-=0.001;
            }

            lastColorUpdate = currentFrame;
        }

        std::cout << r << ' ' << g << ' ' << b << '\n';
        
        RenderText(textShader, "ermm ...", 540.0f, 570.0f, 0.5f,
        glm::vec3(0.3, 0.7f, 0.9f), textVAO, textVBO);
        RenderText(textShader, "what the sigma ?!", 25.0f, 25.0f, 1.0f,
        glm::vec3(r, g, b), textVAO, textVBO);

        glfwSwapBuffers(window);
        glfwPollEvents();
    }

    FT_Done_Face(face);
    FT_Done_FreeType(ft);
    glfwTerminate();
    return 0;
}

void processInput(GLFWwindow* window) {
    if(glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
        glfwSetWindowShouldClose(window, true);

    if(glfwGetKey(window, GLFW_KEY_W) == GLFW_PRESS)
        camera.ProcessKeyboard(FORWARD, deltaTime);
    if(glfwGetKey(window, GLFW_KEY_A) == GLFW_PRESS)
        camera.ProcessKeyboard(LEFT, deltaTime);
    if(glfwGetKey(window, GLFW_KEY_S) == GLFW_PRESS)
        camera.ProcessKeyboard(BACKWARD, deltaTime);
    if(glfwGetKey(window, GLFW_KEY_D) == GLFW_PRESS)
        camera.ProcessKeyboard(RIGHT, deltaTime);
}

void framebuffer_size_callback(GLFWwindow* window, int width, int height) {
    glViewport(0, 0, width, height);
}

void mouse_callback(GLFWwindow* window, double xposIn, double yposIn) {
    float xpos = static_cast<float>(xposIn);
    float ypos = static_cast<float>(yposIn);

    if(firstMouse) {
        lastX = xpos;
        lastY = ypos;

        firstMouse = false;
    }

    float xoffset = xpos - lastX;
    float yoffset = ypos - lastY;

    lastX = xpos;
    lastY = ypos;

    camera.ProcessMouseMovement(xoffset, yoffset);
}

void scroll_callback(GLFWwindow* window, double xoffset, double yoffset) {
    camera.ProcessMouseScroll(static_cast<float>(yoffset));
}

unsigned int loadTexture(const char* path) {
    unsigned int textureID;
    glGenTextures(1, &textureID);

    int width, height, nrComponents;
    unsigned char* data = stbi_load(path, &width, &height, &nrComponents, 0);

    if(data) {
        GLenum format;
        if(nrComponents == 1)
            format = GL_RED;
        else if(nrComponents == 3)
            format = GL_RGB;
        else if(nrComponents == 4)
            format = GL_RGBA;
        
        glBindTexture(GL_TEXTURE_2D, textureID);
        glTexImage2D(GL_TEXTURE_2D, 0, format, width, height, 0, format, GL_UNSIGNED_BYTE, data);

        glGenerateMipmap(GL_TEXTURE_2D);

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    
        stbi_image_free(data);
    } else {
        std::cout << "texture failed to load at path: " << path;
        stbi_image_free(data);
    }

    return textureID;
}
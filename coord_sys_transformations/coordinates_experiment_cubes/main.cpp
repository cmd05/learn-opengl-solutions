#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <stb_image.h>

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>

#define GLM_ENABLE_EXPERIMENTAL
#include <glm/gtx/string_cast.hpp>

//#include <learnopengl/filesystem.h>
#include <learnopengl/shader_m.h>

#include <iostream>

void framebuffer_size_callback(GLFWwindow* window, int width, int height);
void processInput(GLFWwindow* window);

// settings
const unsigned int SCR_WIDTH = 800;
const unsigned int SCR_HEIGHT = 600;

unsigned int SCREEN_CURR_WIDTH = SCR_WIDTH;
unsigned int SCREEN_CURR_HEIGHT = SCR_HEIGHT;

int main()
{
    // glfw: initialize and configure
    glfwInit();
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

#ifdef __APPLE__
    glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
#endif

    // glfw window creation
    GLFWwindow* window = glfwCreateWindow(SCR_WIDTH, SCR_HEIGHT, "LearnOpenGL", NULL, NULL);
    if (window == NULL)
    {
        std::cout << "Failed to create GLFW window" << std::endl;
        glfwTerminate();
        return -1;
    }

    glfwMakeContextCurrent(window);
    glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);

    // glad: load all OpenGL function pointers
    if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
    {
        std::cout << "Failed to initialize GLAD" << std::endl;
        return -1;
    }

    // enable depth testing
    glEnable(GL_DEPTH_TEST);

    // build and compile our shader program
    Shader shaderProgram("shader.vs", "shader.fs"); // you can name your shader files however you like

    // set up vertex data
    float vertices[] = {
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, // back face
         0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

        -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
         0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
         0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
         0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
         0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
         0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
         0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
         0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
    };

    // world space positions of our cubes
    glm::vec3 cubePositions[] = {
        glm::vec3(0.0f,  0.0f,  0.0f),
        glm::vec3(2.0f,  5.0f, -15.0f),
        glm::vec3(-1.5f, -2.2f, -2.5f),
        glm::vec3(-3.8f, -2.0f, -12.3f),
        glm::vec3(2.4f, -0.4f, -3.5f),
        glm::vec3(-1.7f,  3.0f, -7.5f),
        glm::vec3(1.3f, -2.0f, -2.5f),
        glm::vec3(1.5f,  2.0f, -2.5f),
        glm::vec3(1.5f,  0.2f, -1.5f),
        glm::vec3(-1.3f,  1.0f, -1.5f)
    };

    // ebo indices sequence
    unsigned int indices[] = {
        0, 1, 2,
    };

    unsigned int VBO, VAO, EBO;
    glGenVertexArrays(1, &VAO);
    glGenBuffers(1, &VBO);
    glGenBuffers(1, &EBO);

    glBindVertexArray(VAO);

    glBindBuffer(GL_ARRAY_BUFFER, VBO);
    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO);
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);

    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 5 * sizeof(float), (void*)0); // position
    glEnableVertexAttribArray(0);

    glVertexAttribPointer(1, 2, GL_FLOAT, GL_FALSE, 5 * sizeof(float), (void*) (3 * sizeof(float))); // texture
    glEnableVertexAttribArray(1);

    unsigned int texture1, texture2;
    
    // texture 1

    glGenTextures(1, &texture1);
    glBindTexture(GL_TEXTURE_2D, texture1);
    // wrapping
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    // filtering
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

    // load iamge, create texture, generate mipmaps
    int width, height, nrChannels;
    stbi_set_flip_vertically_on_load(true);
    unsigned char* data = stbi_load("../../resources/textures/container.jpg", &width, &height, &nrChannels, 0);

    if (data) {
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, data);
        glGenerateMipmap(GL_TEXTURE_2D);
    }
    else {
        std::cout << "failed to load texture" << std::endl;
    }

    stbi_image_free(data);

    // texture 2
    glGenTextures(1, &texture2);
    glBindTexture(GL_TEXTURE_2D, texture2);
    // wrapping
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    // filtering
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    // load image, create texture, generate mipmaps
    data = stbi_load("../../resources/textures/awesomeface.png", &width, &height, &nrChannels, 0);
    if (data) {
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, data);
        glGenerateMipmap(GL_TEXTURE_2D);
    }
    else {
        std::cout << "failed to load texture" << std::endl;
    }
    stbi_image_free(data);

    // tell opengl for each sampler to which texture unit it belongs to (only has to be done once)
    shaderProgram.use();
    shaderProgram.setInt("texture1", 0);
    shaderProgram.setInt("texture2", 1);

    // unbind VBO
    glBindBuffer(GL_ARRAY_BUFFER, 0);

    // glBindVertexArray(0);

    // glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);

    // render loop
    while (!glfwWindowShouldClose(window))
    {
        processInput(window);

        glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        // bind textures to corresponding texture units
        glActiveTexture(GL_TEXTURE0);
        glBindTexture(GL_TEXTURE_2D, texture1);
        glActiveTexture(GL_TEXTURE1);
        glBindTexture(GL_TEXTURE_2D, texture2);

        shaderProgram.use();
        
        // create matrices for coordinate transformations

        glm::mat4 model = glm::mat4(1.0f),
                  view = glm::mat4(1.0f), 
                  projection = glm::mat4(1.0f);

        float angle = (float)(glfwGetTime());
        //angle = glm::radians(60.0f);

        //model = glm::translate(model, glm::vec3(0,0, -3.0f));
        //model = glm::rotate(model, angle, glm::vec3(0, 1.0, 0));
        //view = glm::translate(view, glm::vec3(0.0f, 0.0f, 3.0f)); 

        /// Model Matrix

        glm::mat4 rotation = glm::transpose(glm::mat4{
            cosf(angle), 0.0f, -sinf(angle), 0.0,
            0.0, 1.0, 0.0, 0.0,
            sinf(angle), 0.0f, cosf(angle), 0.0,
            0.0, 0.0, 0.0, 1.0,
        });

        glm::mat4 translate = glm::mat4(1.0f);
        translate[3][2] = -3.0f; // column major layout

        glm::mat4 scale = glm::mat4(1.0f);
        scale[0][0] = scale[1][1] = scale[2][2] = 1.5;

        model = translate * rotation * scale * model;
        
        /// Projection Matrix
        {
            //float n = 1.0f, f = 100.0f, b = -1.0f, t = 1.0f, l = -1.33f, r = 1.33f;
            
            /// glm::frustum
            //projection = glm::frustum(l, r, b, t, n, f);
            
            /// linear z-mapping
            //projection = glm::transpose(glm::mat4 {
            //    2*n / (r-l), 0, (r+l)/(r-l), 0.0f,
            //    0.0f, 2*n/(t-b), (t+b)/(t-b), 0.0f,
            //    0.0f, 0.0f, 2/(n-f), -1.0f,
            //    0.0f, 0.0f, -1.0f, 0.0f 
            //});
            
            /// non-linear z-mapping
            //projection = glm::transpose(glm::mat4 {
            //    2*n / (r-l), 0, (r+l)/(r-l), 0.0f,
            //    0.0f, 2*n/(t-b), (t+b)/(t-b), 0.0f,
            //    0.0f, 0.0f, -(f+n)/(f-n), -(2*f*n)/(f-n),
            //    0.0f, 0.0f, -1.0f, 0.0f

            //    //0.0f, 0.0f, 0.0f, 2.0f, // define clip space as (-2, -2, -2) to (2, 2, 2)
            //});

            /// infinite perspective
            //projection = glm::transpose(glm::mat4{
            //    2 * n / (r - l), 0, (r + l) / (r - l), 0.0f,
            //    0.0f, 2 * n / (t - b), (t + b) / (t - b), 0.0f,
            //    0.0f, 0.0f, -1.0f, -2*n,
            //    0.0f, 0.0f, -1.0f, 0.0f
            //});

            /// By vertical FOV
            float n = 1.0f, f = 100.0f, angle = glm::radians(90.0f), a_r = (float) SCREEN_CURR_WIDTH / SCREEN_CURR_HEIGHT;
            float tan_half_angle = tanf(angle / 2);

            float top = n * tan_half_angle;
            float right = top * a_r;
            //std::cout << top << " " << right << '\n';

            projection = glm::transpose(glm::mat4{
                1.0f / (tan_half_angle * a_r), 0.0f, 0.0f, 0.0f,
                0.0f, 1.0f / tan_half_angle, 0.0f, 0.0f,
                0.0f, 0.0f, -(f + n) / (f - n), -(2 * f * n) / (f - n),
                0.0f, 0.0f, -1.0f, 0.0f
            });

            /// By glm::perspective
            //projection = glm::perspective(glm::radians(90.0f), (float)SCREEN_CURR_WIDTH / (float)SCREEN_CURR_HEIGHT, 1.0f, 100.0f);
            //std::cout << glm::to_string(projection) << std::endl;

            /// Orthographic Projection
            //float n = 1.0f, f = 100.0f, b = -1.0f, t = 1.0f, l = -1.33f, r = 1.33f;

            //projection = glm::transpose(glm::mat4{
            //    2.0 / (r - l), 0.0, 0.0, 0.0,
            //    0.0, 2.0/(t-b), 0.0,0.0,
            //    0.0, 0.0, -2 / (f-n), -(f+n)/(f-n),
            //    0.0,0.0,0.0,1.0
            //});
        }

        shaderProgram.setMat4("model", model);
        shaderProgram.setMat4("view", view);
        shaderProgram.setMat4("projection", projection);
        
        glBindVertexArray(VAO);
        glDrawArrays(GL_TRIANGLES, 0, 36);
        //glDrawElements(GL_TRIANGLES, 3, GL_UNSIGNED_INT, 0);

        glfwSwapBuffers(window);
        glfwPollEvents();
    }

    glDeleteVertexArrays(1, &VAO);
    glDeleteBuffers(1, &VBO);
    glDeleteBuffers(1, &EBO);

    glfwTerminate();
    return 0;
}

// process all input: query GLFW whether relevant keys are pressed/released this frame and react accordingly
void processInput(GLFWwindow* window)
{
    if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
        glfwSetWindowShouldClose(window, true);
}

// glfw: whenever the window size changed (by OS or user resize) this callback function executes
void framebuffer_size_callback(GLFWwindow* window, int width, int height)
{
    // make sure the viewport matches the new window dimensions; note that width and 
    // height will be significantly larger than specified on retina displays.
    glViewport(0, 0, width, height);

    SCREEN_CURR_WIDTH = width;
    SCREEN_CURR_HEIGHT = height;
}
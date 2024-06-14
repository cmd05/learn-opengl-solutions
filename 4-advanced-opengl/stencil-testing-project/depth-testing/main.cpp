#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <stb_image.h>

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>

#include <learnopengl/shader.h>
#include <learnopengl/camera.h>
#include <learnopengl/model.h>

#include <irrklang/irrKlang.h>
using namespace irrklang;

#include <iostream>
#include <vector>
#include <string>

void framebuffer_size_callback(GLFWwindow* window, int width, int height);
void mouse_callback(GLFWwindow* window, double xpos, double ypos);
void scroll_callback(GLFWwindow* window, double xoffset, double yoffset);
unsigned int loadTexture(const char* path);
void processInput(GLFWwindow* window);

// settings
const unsigned int SCR_WIDTH = 800;
const unsigned int SCR_HEIGHT = 600;

unsigned int CURR_SCR_WIDTH = SCR_WIDTH;
unsigned int CURR_SCR_HEIGHT = SCR_HEIGHT;

Camera camera(glm::vec3(0.0f, 1.0f, 3.0f));
float lastX = (float)SCR_WIDTH / 2.0;
float lastY = (float)SCR_HEIGHT / 2.0;
bool firstMouse = true;

// timing
float deltaTime = 0.0f;
float lastFrame = 0.0f;

// gun selection
int gun_selected = 1;


/// Audio

class Audio {
public:
	Audio() {
		SoundEngine = createIrrKlangDevice();
		is_playing = false;
	}

	~Audio() {
		// Clean up
		curr_sound->drop();
		SoundEngine->drop();
	}

	ISoundEngine* SoundEngine;
	ISound* curr_sound;
	bool is_playing;
};

Audio audio_obj;

class audio_stop_cb : public ISoundStopEventReceiver
{
public:
	// This method is called when a sound stops
	virtual void OnSoundStopped(ISound* sound, E_STOP_EVENT_CAUSE reason, void* userData) override
	{
		// Check if the sound finished playing
		if (reason == E_STOP_EVENT_CAUSE::ESEC_SOUND_FINISHED_PLAYING)
		{
			audio_obj.is_playing = false;
			//printf("Sound finished playing: %s\n", sound->getSoundSource()->getName());
		}
	}
};

audio_stop_cb* eventReceiver = new audio_stop_cb();

int main() {
	glfwInit();
	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

#ifdef __APPLE__
	glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
#endif

	GLFWwindow* window = glfwCreateWindow(SCR_WIDTH, SCR_HEIGHT, "Stencing Testing", NULL, NULL);
	if (window == NULL) {
		std::cout << "Failed to create glfw window" << std::endl;
	}

	glfwMakeContextCurrent(window);
	glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);
	glfwSetCursorPosCallback(window, mouse_callback);
	glfwSetScrollCallback(window, scroll_callback);

	glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);

	if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress)) {
		std::cout << "Failed to initialize GLAD";
		return -1;
	}

	glEnable(GL_DEPTH);
	glDepthFunc(GL_LESS);

	stbi_set_flip_vertically_on_load(true);
	
	Shader normalShader("shader.vert", "shader.frag");
	Shader gunShader("shader.vert", "shader_gun.frag");
	Shader outlineShader("shader.vert", "shader_outline.frag");

	Model wallModel ("../../resources/fps-scene/wall/wall_offset.obj");
	Model floorModel ("../../resources/fps-scene/floor/floor_offset.obj");
	
	std::string gun_path = "../../resources/fps-scene/guns/m4a1/m4a1.obj";
	std::vector<Model> guns (3, gun_path);
	std::vector<glm::vec4> gun_colors = {glm::vec4(0.0), glm::vec4(0.5, 0.1, 0.9, 1.0), glm::vec4(0.6, 0.3, 0.1, 1.0)};

	glEnable(GL_STENCIL_TEST); // enable stencil testing
	glStencilFunc(GL_ALWAYS, 0, 0xFF); // always pass, ref=0
	glStencilOp(GL_KEEP, GL_KEEP, GL_REPLACE); // if both depth and stencil test pass, set stencil_value = 1

	while (!glfwWindowShouldClose(window)) {
		float currentTime = static_cast<float>(glfwGetTime());
		deltaTime = currentTime - lastFrame;
		lastFrame = currentTime;

		processInput(window);

		glStencilMask(0xFF); // enable stencil buffer writing so that stencil buffer can be cleared

		glClearColor(0.05, 0.05, 0.05, 1.0);
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT);

		glStencilMask(0x00); // disable stencil buffer writing

		normalShader.use();

		glm::mat4 model, view, projection;
		model = view = projection = glm::mat4(1.0);

		model = glm::scale(model, glm::vec3(1.0f, 1.0, 1.0));

		view = camera.GetViewMatrix();
		projection = glm::perspective(glm::radians(camera.Zoom), (float)CURR_SCR_WIDTH / (float)CURR_SCR_HEIGHT, 0.1f, 100.0f);

		normalShader.setMat4("model", model);
		normalShader.setMat4("view", view);
		normalShader.setMat4("projection", projection);

		floorModel.Draw(normalShader);
		wallModel.Draw(normalShader);

		// render the same gun because im tired of searching for models with textures that work
		gunShader.use(); // important

		gunShader.setMat4("view", view);
		gunShader.setMat4("projection", projection);

		float gun_offset_y = 0;
		float selected_offset_y = 0;
		
		for (int i = 0; i < guns.size(); i++) {
			model = glm::mat4(1.0);
			model = glm::translate(model, glm::vec3(0.0, 1.0 + gun_offset_y, 0.2));
			model = glm::rotate(model, glm::radians(90.0f), glm::vec3(0.0, 1.0, 0.0));
			model = glm::scale(model, glm::vec3(0.2));

			gunShader.setMat4("model", model);
			gunShader.setVec4("gun_color", gun_colors[i]);

			if (i == (gun_selected - 1)) {
				glStencilFunc(GL_NOTEQUAL, 1, 0xFF); // draw the gun (ref = 1)
				glStencilMask(0xFF); // enable writing to stencil buffer

				selected_offset_y = gun_offset_y;
				
				guns[i].Draw(gunShader);
	
				glStencilMask(0x00); // disable stencil buffer writing
			}
			else {
				guns[i].Draw(gunShader);
			}

			gun_offset_y += 1.0;
		}


		// draw the outline
		glStencilFunc(GL_NOTEQUAL, 1, 0xFF); // pass when value is not 1
		glStencilMask(0x00); // disable writing to stencil buffer

		int selected_index = gun_selected - 1;

		outlineShader.use();

		outlineShader.setMat4("view", view);
		outlineShader.setMat4("projection", projection);

		float scale_gun = 0.202f;
		model = glm::mat4(1.0);
		model = glm::translate(model, glm::vec3(0.0, 1.0 + selected_offset_y, 0.2));
		model = glm::rotate(model, glm::radians(90.0f), glm::vec3(0.0, 1.0, 0.0));
		model = glm::scale(model, glm::vec3(scale_gun));

		outlineShader.setMat4("model", model);

		guns[selected_index].Draw(outlineShader);

		glStencilFunc(GL_ALWAYS, 0, 0xFF); // always pass, ref=0

		glfwSwapBuffers(window);
		glfwPollEvents();
	}

	glfwTerminate();
	return 0;
}

void gunSelectionSound() {
	if (audio_obj.curr_sound) {
		audio_obj.curr_sound->stop();
		audio_obj.curr_sound->drop();
	}

	audio_obj.curr_sound = audio_obj.SoundEngine->play2D("../../resources/fps-scene/audio/gun-select.wav", false, false, true);
	audio_obj.is_playing = true;

	if (audio_obj.curr_sound)
	{
		// Attach the event receiver
		audio_obj.curr_sound->setSoundStopEventReceiver(eventReceiver);
	}
}

void processInput(GLFWwindow* window) {
	if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
		glfwSetWindowShouldClose(window, true);

	// camera logic

	if (glfwGetKey(window, GLFW_KEY_W) == GLFW_PRESS)
		camera.ProcessKeyboard(FORWARD, deltaTime);
	if (glfwGetKey(window, GLFW_KEY_A) == GLFW_PRESS)
		camera.ProcessKeyboard(LEFT, deltaTime);
	if (glfwGetKey(window, GLFW_KEY_S) == GLFW_PRESS)
		camera.ProcessKeyboard(BACKWARD, deltaTime);
	if (glfwGetKey(window, GLFW_KEY_D) == GLFW_PRESS)
		camera.ProcessKeyboard(RIGHT, deltaTime);

	if (camera.Position.z < 0.2)
		camera.Position.z = 0.2;
	if (camera.Position.z > 4.0)
		camera.Position.z = 4.0;

	if (camera.Position.x > 2.0)
		camera.Position.x = 2.0;
	if (camera.Position.x < -2.0)
		camera.Position.x = -2.0;

	if (camera.Position.y != 2.0)
		camera.Position.y = 2.0;

	// gun selection logic
	if (glfwGetKey(window, GLFW_KEY_1) == GLFW_PRESS) {
		gun_selected = 1;
		gunSelectionSound();
	}
	if (glfwGetKey(window, GLFW_KEY_2) == GLFW_PRESS) {
		gun_selected = 2;
		gunSelectionSound();
	}
	if (glfwGetKey(window, GLFW_KEY_3) == GLFW_PRESS) {
		gun_selected = 3;
		gunSelectionSound();
	}
}

void framebuffer_size_callback(GLFWwindow* window, int width, int height) {
	glViewport(0, 0, width, height);
	CURR_SCR_HEIGHT = height;
	CURR_SCR_WIDTH = width;
}

void mouse_callback(GLFWwindow* window, double xposIn, double yposIn) {
	float xpos = static_cast<float>(xposIn);
	float ypos = static_cast<float>(yposIn);

	if (firstMouse) {
		lastX = xpos;
		lastY = ypos;
		firstMouse = false;
	}

	float xoffset = xpos - lastX;
	float yoffset = lastY - ypos; // reversed since y coordinates go from bottom to top

	lastX = xpos;
	lastY = ypos;

	camera.ProcessMouseMovement(xoffset, yoffset);
}

void scroll_callback(GLFWwindow* window, double xoffset, double yoffset)
{
	camera.ProcessMouseScroll(static_cast<float>(yoffset));
}
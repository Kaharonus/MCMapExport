#version 330 core

layout (location = 0) in vec3 aPos;

uniform float camX;
uniform float camY;
uniform float camZoom;
uniform float aspectRatio;
uniform float xOffset;
uniform float yOffset;

void main() {

    float x = (((aPos.x + xOffset)/aspectRatio)*camZoom) + camX;
    float y = ((aPos.y + yOffset) * camZoom) + camY;
    gl_Position = vec4(x, y, aPos.z * camZoom, 1.0);
}


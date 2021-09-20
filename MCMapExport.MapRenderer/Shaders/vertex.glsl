#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 texCoord;


uniform float camX;
uniform float camY;
uniform float camZoom;
uniform float aspectRatio;
uniform float xOffset;
uniform float yOffset;

out vec2 texcoords;

void main() {

    float x = (((aPos.x + xOffset)/aspectRatio) + camX) *camZoom;
    float y = ((aPos.y + yOffset)  + camY) * camZoom;
    gl_Position = vec4(x, y, aPos.z * camZoom, 1.0);
    texcoords = texCoord;
}


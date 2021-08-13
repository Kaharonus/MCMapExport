#version 330 core

layout (location = 0) in vec3 aPos;
out vec2 texcoords; // texcoords are in the normalized [0,1] range for the viewport-filling quad part of the triangle

uniform float camX;
uniform float camY;
uniform float camZoom;
uniform float aspectRatio;

void main() {
    gl_Position = vec4(((aPos.x/aspectRatio)*camZoom) + camX, (aPos.y* camZoom) + camY, aPos.z * camZoom, 1.0);
}


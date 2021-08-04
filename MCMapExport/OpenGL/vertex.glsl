#version 330 core

layout (location = 0) in vec3 aPos;
out vec2 texcoords; // texcoords are in the normalized [0,1] range for the viewport-filling quad part of the triangle
void main() {

    vec2 vertices[3]=vec2[3](vec2(-1,-1), vec2(3,-1), vec2(-1, 3));
    gl_Position = vec4(vertices[gl_VertexID],0,1);
    texcoords = 0.5 * gl_Position.xy + vec2(0.5);
}



/*#version 330 core
layout (location = 0) in vec3 aPos; // the position variable has attribute position 0
  
out vec4 vertexColor; // specify a color output to the fragment shader

void main()
{
    gl_Position = vec4(aPos, 1.0); // see how we directly give a vec3 to vec4's constructor
    vertexColor = vec4(0.5, 0.0, 0.0, 1.0); // set the output variable to a dark-red color
}*/
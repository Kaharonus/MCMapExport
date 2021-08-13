
#version 330 core
out vec4 FragColor;
 
 
in vec4 gl_FragCoord;
in vec2 texcoords;

uniform sampler2D tex;

  
void main(){
    FragColor = texture(tex, texcoords);
    FragColor = vec4(1,1,1,1);
} 
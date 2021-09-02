
#version 330 core
out vec4 FragColor;
 
 
in vec4 gl_FragCoord;
in vec2 texcoords;

uniform sampler2D tex;

  
void main(){
    //FragColor = texture(tex, texcoords);
    vec4 texColor = texture(tex, texcoords);
    //FragColor = mix(vec4(0.2), texColor, texColor.a);
    FragColor = mix(texColor, vec4(0.5,0.5,0.5,1), 0.2);
    //FragColor = mix(vec4(texcoords.x, texcoords.y, 0 , 1), vec4(0.5,0.5,0.5,1), 0.2);
    //FragColor = vec4(1,1,1,1);
} 
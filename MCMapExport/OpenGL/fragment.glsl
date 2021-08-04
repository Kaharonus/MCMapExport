
#version 330 core
out vec4 FragColor;
 
 
in vec4 gl_FragCoord;
in vec2 texcoords;

uniform float width;
uniform float height;

uniform sampler2D tex;

  
void main(){
    FragColor = texture(tex, texcoords);
   
    //FragColor = vec4(texcoords.x, texcoords.y, 0,1);
    
    /*vec2 st = vec2(texcoords.x, texcoords.y);

    vec3 color1 = vec3(1.0,0.55,0);
    vec3 color2 = vec3(0.226,0.000,0.615);

    float mixValue = distance(st,vec2(0,1));
    vec3 color = mix(color1,color2,mixValue);

    //gl_FragColor = vec4(color,mixValue);
    FragColor = vec4(color,1.0);
    //FragColor = vec4(texcoords / (width + height), 1.0,1.0);
    //FragColor = vec4(0.5, 0.1, 0.1, 1.0);*/
} 
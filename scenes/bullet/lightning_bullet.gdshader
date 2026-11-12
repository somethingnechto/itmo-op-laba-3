shader_type spatial;

uniform vec3 base_color : source_color = vec3(1.0, 1.0, 1.0);
uniform vec3 emission_color : source_color = vec3(1.0, 1.0, 0.0);
uniform float emission_energy = 1.0;

uniform float jagged_amount = 0.05;
uniform float jagged_frequency = 15.0;
uniform float speed = 3.0;

void vertex() {
	float wave = sin(VERTEX.x * jagged_frequency + TIME * speed) 
               * cos(VERTEX.y * jagged_frequency + TIME * speed) 
               * sin(VERTEX.z * jagged_frequency + TIME * speed);
    
    VERTEX += NORMAL * wave * jagged_amount;
}

void fragment() {
    ALBEDO = base_color;
    EMISSION = emission_color * emission_energy;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}

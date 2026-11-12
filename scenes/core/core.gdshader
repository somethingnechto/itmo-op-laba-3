shader_type spatial;

void fragment() {
	ALBEDO = vec3(sin(TIME * 2.0) * 0.8, sin(TIME * 2.0) * 0.8, 1.0);
	EMISSION = vec3(1.0, 1.0, 1.0);
}

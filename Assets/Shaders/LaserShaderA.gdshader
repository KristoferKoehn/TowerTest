shader_type spatial;
render_mode unshaded;

uniform vec3 LaserCoreColor : source_color = vec3(0,0,0);

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	ALBEDO = LaserCoreColor + 5.0 * normalize(LaserCoreColor) * (0.15 / 0.15 - (abs(dot( NORMAL, VIEW ))));
	// Called for every pixel the material is visible on.
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}

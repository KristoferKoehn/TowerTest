shader_type canvas_item;

uniform float AlphaScalar = 1.0;
uniform float AlphaCieling = 0.2;
uniform float Amplitude = 0.07;
void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	float animate = abs(pow(UV.x - 0.5, 2.0) + pow(UV.y - 0.5, 2.0)) - AlphaCieling - Amplitude * sin(TIME);
	COLOR.a = animate * COLOR.a * AlphaScalar;
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}

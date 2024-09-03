shader_type spatial;
render_mode cull_disabled, unshaded, depth_draw_never;

uniform float size = 0;
uniform vec4 HighlightColor : source_color = vec4(1.0, 1.0, 1.0, 1.0) ;
void vertex() {
	VERTEX =  (NODE_POSITION_WORLD * size) - (NODE_POSITION_WORLD - VERTEX) * size;
	NORMAL = -NORMAL;
	// Called for every vertex the material is visible on.
}

void fragment() {
	ALBEDO = HighlightColor.xyz;
	ALPHA = HighlightColor.w;
	// Called for every pixel the material is visible on.
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}

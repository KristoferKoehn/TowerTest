shader_type spatial;
render_mode cull_disabled, unshaded;

uniform sampler2D noise;
uniform vec4 color : source_color = vec4(1.0);
uniform float Alpha;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	float alpha = 0.0;
	if (UV.y < 0.1) {
		alpha = texture(noise, vec2(UV.x , UV.y+TIME/2.0)).r * UV.y * 10.0;
		//feather from nothing (UV.y == 0) to full (UV.y == 0.1)
	} else if (UV.y > 0.4) {
		alpha = texture(noise, vec2(UV.x , UV.y+TIME/2.0)).r * (0.5 - UV.y) * 10.0;
	} else {
		alpha = texture(noise, vec2(UV.x , UV.y+TIME/2.0)).r;
	}
	
	ALPHA = Alpha * alpha;
	ALBEDO = color.rgb;
	
	// Called for every pixel the material is visible on.
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}

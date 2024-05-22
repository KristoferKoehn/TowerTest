shader_type spatial;

uniform float width;
uniform float radius;
uniform vec4 trail_color : source_color = vec4(1,0,1,1); 

void fragment() {
	if (length(UV - vec2(0.5, 0.5)) > radius - width/2.0 && length(UV - vec2(0.5, 0.5)) < radius + width) {
		ALBEDO = trail_color.rgb;
		ALPHA = trail_color.a;
	} else {
		ALPHA = 0.0;
	}
}

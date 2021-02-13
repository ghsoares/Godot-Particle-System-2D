shader_type canvas_item;

uniform sampler2D gradient;

varying vec2 v;

void vertex() {
	v = VERTEX;
}

void fragment() {
	COLOR = texture(gradient, vec2(UV.x, 0f));
	
	float sqX = floor(v.x / 4f);
	float sqY = floor(v.y / 4f);
	
	float sq = mod(sqX + sqY, 2);
	
	vec4 sqCol = mix(vec4(1f), vec4(.5f), sq);
	COLOR = mix(COLOR, sqCol, 1f - COLOR.a);
}


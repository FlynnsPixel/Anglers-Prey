Shader "FX/Water (Basic)" {

	Properties {
		_colour("Colour", Color) = (0, 0, 1, 1)
	}
	SubShader {
		Pass {
			Material {
				Diffuse [_colour]
			}
			Lighting on
		}
	}
}
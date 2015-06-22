Shader "Custom/UnderShader" {

	Properties {
		colour_overlay("Colour overlay", COLOR) = (0, 0, 0, 1)
		wave_scale("Wave scale", Range (0.02,1)) = .3
		reflect_colour("Reflective colour (RGB) fresnel (A) ", 2D) = "" { }
		bump_map("Waves Normalmap ", 2D) = "" { }
		wave_speed("Wave speed (map1 x,y; map2 x,y)", Vector) = (8, 8, 4, -8)
	}

	Subshader {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			uniform float4 colour_overlay;

			uniform float4 wave_speed;
			uniform float wave_scale;
			uniform float4 wave_offset;

			uniform sampler2D light_data;
			uniform int num_lights = 0;
			uniform float next_light_uv;
			
			sampler2D bump_map;
			sampler2D reflect_colour;

			#include "UnityCG.cginc"

			struct v2f {
	            float4 pos : SV_POSITION;
	            float4 colour : COLOR;
	            float2 bumpuv0 : TEXCOORD0;
				float2 bumpuv1 : TEXCOORD1;
				float3 v_dir : TEXCOORD2;
				float2 uv : TEXCOORD3;
	        };

	        struct VertIn {
            	float4 vertex : POSITION;
            	float4 color : COLOR;
            	float2 texcoord : TEXCOORD;
         	};

	        v2f vert(VertIn v) {
	            v2f o;
	        	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	        	o.colour = v.color;
				o.uv = v.texcoord;

	            return o;
	        }

	        fixed4 frag(v2f i) : SV_Target {
	        	half4 col = half4(1, 0, 0, 1);
	        	
	            return col;
	        }

			ENDCG
		}
	}
}

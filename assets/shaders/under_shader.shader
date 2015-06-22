Shader "Custom/UnderShader" {

	Properties {
		colour_overlay2("Colour overlay", COLOR) = (0, 0, 0, 1)
		bump_map2("Waves Normalmap ", 2D) = "" { }
	}

	Subshader {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			sampler2D bump_map2;

			#include "UnityCG.cginc"

			struct v2f {
	            float4 pos : SV_POSITION;
	            float4 colour : COLOR;
				float2 uv : TEXCOORD;
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

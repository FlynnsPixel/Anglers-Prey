Shader "Custom/Ripple" {

	Properties {
		_MainTex ("", 2D) = "white" {}
	}

	Subshader {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f {
	            float4 pos : SV_POSITION;
	            float4 colour : COLOR;
				float2 screen_uv : TEXCOORD0;
	        };

	        struct VertIn {
            	float4 vertex : POSITION;
            	float4 color : COLOR;
            	float2 texcoord : TEXCOORD0;
         	};

	        v2f vert(VertIn v) {
	            v2f o;
	        	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	        	o.screen_uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord.xy);

	            return o;
	        }

	        sampler2D _MainTex;

	        fixed4 frag(v2f i) : SV_Target {
				float2 tc = i.screen_uv.xy;
			  	float2 p = tc.xy;
			  	float len = length(p);
			  	float dist = sqrt(pow(p, 2) + pow(p, 2)) + .5;
			  	float ripple = (p / len) * sin((len * 50.0) - (_Time.y * 6.0)) * .004;
			  	float2 uv = tc + ripple;
				half4 col = tex2D(_MainTex, uv);
				
	            return col;
	        }

			ENDCG
		}
	}
}

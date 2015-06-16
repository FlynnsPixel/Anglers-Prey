Shader "Custom/Map" {

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
			uniform int num_lights;
			uniform float next_light_uv;
			
			sampler2D bump_map;
			sampler2D reflect_colour;

			#include "UnityCG.cginc"

			struct v2f {
	            float4 pos : SV_POSITION;
	            fixed3 colour : COLOR0;
	            float2 bumpuv0 : TEXCOORD0;
				float2 bumpuv1 : TEXCOORD1;
				float3 v_dir : TEXCOORD2;
				float2 uv : TEXCOORD3;
	        };

	        v2f vert(appdata_base v) {
	            v2f o;
	        	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

				//scroll bump waves
				float4 temp;
				temp.xyzw = v.vertex.xzxz * wave_scale / 1.0 + wave_offset;
				o.bumpuv0 = temp.xy * float2(.4, .45);
				o.bumpuv1 = temp.wz;
				o.uv = v.vertex.zx * wave_scale / 1.0;

				//object space view direction
				o.v_dir = normalize(ObjSpaceViewDir(v.vertex)).xzy;

	            return o;
	        }

	        fixed4 frag(v2f i) : SV_Target {
	        	half3 bump1 = UnpackNormal(tex2D(bump_map, i.bumpuv0)).rgb;
				half3 bump2 = UnpackNormal(tex2D(bump_map, i.bumpuv1)).rgb;
				half3 bump = (bump1 + bump2) * 0.5;
				
				half fresnel = dot(i.v_dir, bump);
				half4 water = tex2D(reflect_colour, float2(fresnel, fresnel));

				half4 col = half4(lerp(water.rgb, colour_overlay.rgb, water.rgb) - 1, 1);

	        	float2 light_uv;
	        	light_uv = 0;
	        	half2 origin_uv = i.uv;
	        	for (int n = 0; n < num_lights; ++n) {
	        		half4 attr1 = tex2D(light_data, light_uv);
	        		light_uv.x += next_light_uv;
	        		half4 attr2 = tex2D(light_data, light_uv);
	        		light_uv.x += next_light_uv;
	        		half4 attr3 = tex2D(light_data, light_uv);
	        		light_uv.x += next_light_uv;
	        		
	        		i.uv.y += ((attr3.r * 255) - 127) + attr3.g;
	        		i.uv.x += ((attr3.b * 255) - 127) + attr3.a;

		        	float dist = sqrt(pow(i.uv.x, 2) + pow(i.uv.y, 2));
		        	float size = attr2.r;
		        	float intensity = attr2.g * 64;
		        	dist = clamp(intensity - (dist / (size / intensity)), 0, intensity);
		        	col.r += dist * attr1.r;
		        	col.g += dist * attr1.g;
		        	col.b += dist * attr1.b;
		        	col.a += dist * col.rgb / 10.0;

		        	i.uv = origin_uv;
	        	}
	        	
	            return col;
	        }

			ENDCG
		}
	}
}
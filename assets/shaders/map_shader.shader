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

		Blend SrcColor SrcColor

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

			float4 reflect_colour_ST;

			struct v2f {
	            float4 pos : SV_POSITION;
	            float4 colour : COLOR;
	            float2 bumpuv0 : TEXCOORD0;
				float2 bumpuv1 : TEXCOORD1;
				float3 v_dir : TEXCOORD2;
				float2 uv : TEXCOORD3;
				float2 screen_uv : TEXCOORD4;
	        };

	        struct VertIn {
            	float4 vertex : POSITION;
            	float4 color : COLOR;
         	};

	        v2f vert(VertIn v) {
	            v2f o;
	        	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	        	o.colour = v.color;
	        	o.screen_uv = o.pos.xy;

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

				float2 tc = i.uv.xy;
			  	float2 p = tc;
			  	p.x += _Time.y / 8.0;
			  	float len = length(p);
			  	float dist = sqrt(pow(p, 2) + pow(p, 2)) + .5;
			  	float ripple = (p / len) * sin((len * 15.0) - (_Time.y * 12.0)) / (dist * 20.0);
			  	float2 uv = float2(fresnel, fresnel);
				//half3 col2 = tex2D(reflect_colour, uv).xyz;

				half4 water = tex2D(reflect_colour, uv);

				half4 col = half4(lerp(water.rgb, colour_overlay.rgb, water.rgb) - .2, 1);

	        	float2 light_uv = 0;
	        	half2 origin_uv = i.uv;
	        	for (int n = 0; n < num_lights; ++n) {
	        		half4 attr1 = tex2D(light_data, light_uv);
	        		light_uv.x += next_light_uv;
	        		half4 attr2 = tex2D(light_data, light_uv);
	        		light_uv.x += next_light_uv;
	        		half4 attr3 = tex2D(light_data, light_uv);
	        		light_uv.x += next_light_uv;
	        		
	        		i.uv.y -= ((attr3.r * 255) - 127) + attr3.g;
	        		i.uv.x -= ((attr3.b * 255) - 127) + attr3.a;

		        	float dist = sqrt(pow(i.uv.x, 2) + pow(i.uv.y, 2));
		        	float size = (attr2.r * 255) + attr2.g;
		        	float intensity = (attr2.b * 64) * 3;
		        	dist = clamp(intensity - (dist / (size / intensity)), 0, intensity);
		        	col.rgb += (dist * (attr1.rgb / 3)) * attr1.a;

		        	i.uv = origin_uv;
	        	}

	        	col.rgb += i.colour.rgb;
				col.rgb -= clamp(sqrt(pow(i.screen_uv.x, 2) + pow(i.screen_uv.y, 2)) / 75, 0, 1);
				col.rgb *= colour_overlay.a;
				col.r = clamp(col.r, 0, .9);
				col.g = clamp(col.g, 0, .9);
				col.b = clamp(col.b, 0, .9);

				//col.rgb = col2.rgb;

	            return col;
	        }

			ENDCG
		}
	}
}

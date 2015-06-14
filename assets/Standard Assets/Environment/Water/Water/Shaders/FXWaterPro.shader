#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'

Shader "FX/Water (simple)" {
Properties {
	_horizonColor ("Horizon color", COLOR)  = ( .172 , .463 , .435 , 0)
	_WaveScale ("Wave scale", Range (0.02,1)) = .07
	_ColorControl ("Reflective color (RGB) fresnel (A) ", 2D) = "" { }
	_ColorControlCube ("Reflective color cube (RGB) fresnel (A) ", Cube) = "" { TexGen CubeReflect }
	_BumpMap ("Waves Normalmap ", 2D) = "" { }
	WaveSpeed ("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
	_MainTex ("Fallback texture", 2D) = "" { }
	
	_Specular ("Specular", Range (0,1)) = .07
	_Gloss ("Gloss", Range (0,128)) = 1
}

	
// -----------------------------------------------------------
// Fragment program

Subshader {
	Tags { "Queue"="Transparent" "RenderType"="Transparent" }
	
	Blend SrcAlpha OneMinusSrcAlpha
	Blend One SrcAlpha

	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		uniform float4 _horizonColor;

		uniform float4 WaveSpeed;
		uniform float _WaveScale;
		uniform float4 _WaveOffset;
		
		uniform float _Specular;
		uniform float _Gloss;
		
		sampler2D _BumpMap;
		sampler2D _ColorControl;

		#include "UnityCG.cginc"

		struct v2f {
            float4 pos : SV_POSITION;
            fixed3 colour : COLOR0;
            float2 bumpuv0 : TEXCOORD0;
			float2 bumpuv1 : TEXCOORD1;
			float3 vDir : TEXCOORD2;
			float2 uv : TEXCOORD3;
        };

        float4 _BumpMap_ST;
		float4 _ColorControl_ST;

        v2f vert(appdata_base v) {
            v2f o;
        	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

        	float4 s;

			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);

			// scroll bump waves
			float4 temp;
			temp.xyzw = v.vertex.xzxz * _WaveScale / 1.0 + _WaveOffset;
			o.bumpuv0 = temp.xy * float2(.4, .45);
			o.bumpuv1 = temp.wz;
			o.uv = v.vertex.zx * _WaveScale / 1.0;

			// object space view direction
			o.vDir = normalize(ObjSpaceViewDir(v.vertex)).xzy;

            return o;
        }

        fixed4 frag(v2f i) : SV_Target {
        	half3 bump1 = UnpackNormal(tex2D( _BumpMap, i.bumpuv0 )).rgb;
			half3 bump2 = UnpackNormal(tex2D( _BumpMap, i.bumpuv1 )).rgb;
			half3 bump = (bump1 + bump2) * 0.5;
			
			half fresnel = dot( i.vDir, bump);
			half4 water = tex2D( _ColorControl, float2(fresnel,fresnel) );
			
			half4 col;
			col.rgb = water.rgb;
			
        	//i.uv.x -= .5;
        	//i.uv.y -= .5;
        	//col.rgb -= 1;
        	//col.r += (1 - sqrt(dot(i.uv * 2, i.uv * 2))) * 2;

            return col;
        }

		ENDCG
	}
}
}

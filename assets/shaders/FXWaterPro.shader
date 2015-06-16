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

CGPROGRAM
		#pragma surface surf SimpleSpecular vertex:vert Lambert alpha
		
		uniform float4 _horizonColor;

		uniform float4 WaveSpeed;
		uniform float _WaveScale;
		uniform float4 _WaveOffset;
		
		uniform float _Specular;
		uniform float _Gloss;
		
		sampler2D _BumpMap;
		sampler2D _ColorControl;

		#include "UnityCG.cginc"

		struct Input {
			
			float2 bumpuv0 : TEXCOORD0;
			float2 bumpuv1 : TEXCOORD1;
			float3 vDir : TEXCOORD2;
		};
		
		void vert (inout appdata_full v, out Input o) {
			float4 s;

			//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);

			// scroll bump waves
			float4 temp;
			temp.xyzw = v.vertex.xzxz * _WaveScale / 1.0 + _WaveOffset;
			o.bumpuv0 = temp.xy * float2(.4, .45);
			o.bumpuv1 = temp.wz;
			
			// object space view direction
			o.vDir = normalize(ObjSpaceViewDir(v.vertex)).xzy;
		}

		void surf (Input IN, inout SurfaceOutput o) {
		
			half3 bump1 = UnpackNormal(tex2D( _BumpMap, IN.bumpuv0 )).rgb;
			half3 bump2 = UnpackNormal(tex2D( _BumpMap, IN.bumpuv1 )).rgb;
			half3 bump = (bump1 + bump2) * 0.5;
			
			half fresnel = dot( IN.vDir, bump);
			half4 water = tex2D( _ColorControl, float2(fresnel,fresnel) );
			
			half4 col;
			col.rgb = lerp(water.rgb, _horizonColor.rgb, water.a);

			o.Normal = bump;
			o.Albedo = col;
			o.Alpha = _horizonColor.a;
		}
		
		half4 LightingSimpleSpecular (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
          half3 h = normalize (lightDir + viewDir);

          half diff = max (0, dot (s.Normal, lightDir));
          
          float nh = max (0, dot (s.Normal, h));
          float spec = pow (nh, _Gloss);

          half4 c;
          c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten * 2) * _Specular;
          c.a = s.Alpha;
          
          return c;
      }
		ENDCG
}

// -----------------------------------------------------------
//  Old cards

}

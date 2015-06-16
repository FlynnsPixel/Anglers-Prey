Shader "Custom/distortion_shader" {

	Properties {
		colour("Color", Color) = (1,1,1,1)
		tex("Albedo (RGB)", 2D) = "white" {}
		gloss("Smoothness", Range(0,1)) = 0.5
		metallic("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }

      	CGPROGRAM
      	#pragma surface surf Lambert vertex:vert

      	uniform float rand;

        struct VertData {
            float4 vertex : POSITION;
            float4 texcoord : TEXCOORD0;
            float3 normal : NORMAL;
        };

      	struct Input {
        	float2 tex_uv;
      	};

      	void vert (inout VertData v) {	
      		v.vertex.x += cos(cos(_Time.y) + v.vertex.x - v.vertex.z + rand) / 4;
      		v.vertex.z += sin(cos(_Time.y) + v.vertex.x - v.vertex.z + rand) / 4;
      	}

      	sampler2D tex;
      	void surf (Input IN, inout SurfaceOutput o) {
          	o.Albedo = tex2D(tex, IN.tex_uv).rgb;
      	}
      	ENDCG
	}
	FallBack "Diffuse"
}

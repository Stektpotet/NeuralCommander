Shader "MrRogueBot/ColorMasked" {
	Properties {
		[NoAlpha]
		_Color ("Color", Color) = (1,1,1)
		_MainTex ("Albedo (RGBA)", 2D) = "white" {}
		[Normal]
		[NoScaleOffset]
		_BumpTex("Normal (RGB)", 2D) = "white" {}

		[NoScaleOffset]
		_MetallicTex("Metallic (RGBA)", 2D) = "white" {}
		[NoScaleOffset]
		_OcclusionTex("Occlusion (R)", 2D) = "white" {}

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
		};

		sampler2D _MainTex;
		sampler2D _BumpTex;
		sampler2D _MetallicTex; 
		sampler2D _OcclusionTex;
		

		float _Occlusion;
		half _Glossiness;
		half _Metallic;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			// Metallic and smoothness come from slider variables
			fixed4 metallicSmoothness = tex2D(_MetallicTex, IN.uv_MainTex);
			fixed3 nrm = UnpackNormal(tex2D(_BumpTex, IN.uv_MainTex));

			fixed mask = saturate(c.a - metallicSmoothness.a);

			fixed3 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color).rgb;

			c.rgb = c.rgb * mask * color.rgb + (1-mask) * c.rgb; //apply color mask


			o.Albedo = c.rgb;//c.rgb;
			o.Occlusion = tex2D(_OcclusionTex, IN.uv_MainTex).a;
			o.Normal = nrm;
			o.Metallic = metallicSmoothness.r * _Metallic;
			o.Smoothness = metallicSmoothness.a * _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

Shader "Obliy/Masked Lit"
{
	Properties
	{
		_ColorTint("Color Tint", Color) = (1,1,1,0)
		_ColorTexture("Color Texture", 2D) = "white" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.15
		_AlphaCutoff("Alpha Cutoff", Range( 0 , 1)) = 0.35
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags
		{ 
			"RenderPipeline" = "UniversalPipeline"
			"RenderType" = "TransparentCutout"
			"Queue" = "AlphaTest"
		}
		
		Pass
		{
			Cull Back
			
			Name "Universal Forward"
            Tags { "LigthMode" = "UniversalForward" }
			
			HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exculde_renderer d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
			
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct VertexOutput
			{
				float4 vertex : SV_POSITION;
				float2 uv_texcoord : TEXCOORD0;
			};

			uniform float4 _ColorTint;
			uniform sampler2D _ColorTexture;
			uniform  float4 _ColorTexture_ST;
			uniform float _Smoothness;
			uniform float _AlphaCutoff;
			
			VertexOutput vert(VertexInput v)
			{
				VertexOutput o;
				o.vertex = TransformObjectToHClip(v.vertex.xyz);
				o.uv_texcoord = v.uv.xy * _ColorTexture_ST.xy + _ColorTexture_ST.zx;
				return  o;
			}

			half4 frag(VertexOutput i) : SV_Target
			{
				float2 uv_ColorTexture = i.uv_texcoord * _ColorTexture_ST.xy + _ColorTexture_ST.zx;
				half4 tex2DNode1 = tex2D(_ColorTexture, uv_ColorTexture);
				float3 albedo = (_ColorTint * tex2DNode1).rgb;
				half4 color = half4(albedo.r, albedo.g, albedo.b, 1);
				clip(tex2DNode1.a - _AlphaCutoff);
				return  color;
			}
			ENDHLSL
		}
	}
}


//		#pragma target 3.0
//		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
//		struct Input
//		{
//			float2 uv_texcoord;
//		};
//
//		uniform float4 _ColorTint;
//		uniform sampler2D _ColorTexture;
//		uniform float4 _ColorTexture_ST;
//		uniform float _Smoothness;
//		uniform float _AlphaCutoff;
//
//		void surf( Input i , inout SurfaceOutputStandard o )
//		{
//			float2 uv_ColorTexture = i.uv_texcoord * _ColorTexture_ST.xy + _ColorTexture_ST.zw;
//			float4 tex2DNode1 = tex2D( _ColorTexture, uv_ColorTexture );
//			o.Albedo = ( _ColorTint * tex2DNode1 ).rgb;
//			o.Smoothness = _Smoothness;
//			o.Alpha = 1;
//			clip( tex2DNode1.a - _AlphaCutoff );
//		}
//
//		ENDCG
//	}
//	Fallback "Diffuse"
//}
Shader "URPTraining/URPBasic02"
{
    Properties
    {
        _TintColor("Color", color) = (1,1,1,1)
        _MainTex ("RGB", 2D) = "white" {}
        _MainTex02 ("RGB02", 2D) = "white" {}
    }
    SubShader
    {
        Tags 
        {
            // Redner type과 Redner Queue를 여기서 결정합니다.
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Opaque" 
            "Queue" = "Geometry"
        }

        Pass
        {
            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exculde_renderer d3d11_9x
            #pragma vertex vert
            #pragma fragment frag

            // CG : shader는 .cginc를 hlsl shader는 .hlsl을 include하게 됩니다.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // vertex buffer에서 읽어올 정보를 선언합니다.
            struct VertexInput
            {
                float4 vertex : POSITION;
                // UV1번을 사용하는 경우
                float2 uv : TEXCOORD0;
                // UV2번을 사용하는 경우
                float2 uv1 : TEXCOORD1;
            };
            
            // 버텍스 셰이더에서 픽셀 셰이더로 전달할 정보를 선언합니다.
            // 보간기 : Vertxt Shader에서 Pixcel Shader로 이동할 때
            // 보간기의 숫자는
            // 
            struct VertexOutput
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Color값은 half로 설정함
            half4 _TintColor;

            float4 _MainTex_ST;
            Texture2D _MainTex;
            Texture2D _MainTex02;

            SamplerState sampler_MainTex;


            // 버텍스 셰이더
            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
            }

            // 픽셀 셰이더(Pixel shader)
            half4 frag(VertexOutput i) : SV_Target
            {
                float4 col01 = _MainTex.Sample(sampler_MainTex, i.uv);
                float4 col02 = _MainTex02.Sample(sampler_MainTex, i.uv);

                half4 color = col01 + col02;
                return color;
            }
            ENDHLSL
        }
    }
}

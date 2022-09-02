Shader "Hidden/CusedWoods/GhostPostProcessShader"
{
  Properties
  {
      _MainTex("Main Texture", 2D) = "white" {}
  }
    SubShader
  {
    Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

    Pass
    {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

      #pragma vertex vert
      #pragma fragment frag

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _Intensity; // 0, .4, .55, .75
            float4 _OverlayColor;

            struct Attributes {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
            };

            struct Varyings {
                float2 uv        : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };


            Varyings vert(Attributes input) {
                Varyings output = (Varyings)0;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertexInput.positionCS;
                output.uv = input.uv;

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                if (_Intensity == 0) return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                float dist = sqrt((input.uv.x - .5) * (input.uv.x - .5) + (input.uv.y - .5) * (input.uv.y - .5)) * 4;

                float2 uv = input.uv;
                uv = (uv - float2(.5,.5)) *  (1 + _Intensity * sqrt(4 - dist) * .1) + float2(.5,.5);

                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                float val = clamp(dist * _Intensity, 0, 1.15);
                return lerp(color, _OverlayColor, val);
            }

      ENDHLSL
    }
  }
    FallBack "Diffuse"
}
Shader "CursedWoods/GhostShader"
{
  Properties
  {
      _MainTex("Texture", 2D) = "white" {}
      _BaseColor("Color", Color) = (1, 1, 1, 1)
      _Alpha("Alpha", float) = 1
      _Bright("Bright", float) = 1
  }
    SubShader
      {
        Tags {"RenderType" = "Opaque"}
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull front
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float _Alpha;
        float _Bright;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);


        struct VertexInput {
          float4 position : POSITION;
          float2 UV : TEXCOORD0;
        };

        struct VertexOutput {
          float4 position : SV_POSITION;
          float2 uv : TEXCOORD0;
        };



        ENDHLSL

        Pass
        {
          HLSLPROGRAM
          #pragma vertex vert
          #pragma fragment frag

          VertexOutput vert(VertexInput i) {
            VertexOutput o;
            o.position = TransformObjectToHClip(i.position.xyz);
            o.uv = i.UV;
            return o;
          }

          float4 frag(VertexOutput i) : SV_Target
          {
            float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
            col = col * _BaseColor;

            if (_Bright > 0) col.rgb = (1 - _Bright) * col.rgb + _Bright * float3(1, 1, 1);
            if (_Bright < 0) col.rgb = (1 - _Bright) * col.rgb + _Bright * float3(0, 0, 0);

            col.a = _Alpha;
            return col;
        }
        ENDHLSL
        }
      }
}

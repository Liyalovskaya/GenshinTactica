Shader "Unlit/WriteMask"
{
    Properties
    {

    }
    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" "ShaderModel" = "4.5"}
        LOD 100

        ZTest LEqual
        ZWrite Off

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            
//            Stencil
//            {
//                Ref 2
//                Comp Equal
//            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            // note: if you're running low on global keywords, swap this out with multi_compile_local
            // and then also change the code in RenderPassOutlineSDF as noted there
            #pragma multi_compile _ _CORGIOUTLINE_USENEARFARCLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _CorgiOutlineNearClipPlane;
            float _CorgiOutlineFarClipPlane;

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;

#ifdef _CORGIOUTLINE_USENEARFARCLIP
                float distance : TEXCOORD0;
#endif

                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs vertInputs = GetVertexPositionInputs(v.vertex.xyz);
                o.vertex = vertInputs.positionCS;

#ifdef _CORGIOUTLINE_USENEARFARCLIP
                o.distance = length(vertInputs.positionWS - _WorldSpaceCameraPos.xyz); 
#endif

                return o;
            }

            float frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

#ifdef _CORGIOUTLINE_USENEARFARCLIP
                if (i.distance < _CorgiOutlineNearClipPlane || i.distance > _CorgiOutlineFarClipPlane)
                {
                    clip(-1); 
                }
#endif

                return 1;
            }

            ENDHLSL
        }
    }
}

Shader "Hidden/URP_BlitSDFToCamera"
{
    HLSLINCLUDE

    #pragma target 5.0
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    #if defined(USING_STEREO_MATRICES) && defined(UNITY_STEREO_INSTANCING_ENABLED)
        #define GET_INVERSE_PROJECTION _CorgiInverseProjectionArray[unity_StereoEyeIndex]
        #define SAMPLE_TEXTURE2D_X_BIAS(textureName, samplerName, coord2, bias)                  textureName.Sample(samplerName, float3(coord2, unity_StereoEyeIndex), bias)
    #else
        #define GET_INVERSE_PROJECTION _CorgiInverseProjection
        #define SAMPLE_TEXTURE2D_X_BIAS(textureName, samplerName, coord2, bias)                  textureName.Sample(samplerName, coord2, bias)
    #endif

    float4 _WorldSpaceLightPos0;

    struct AttributesDefault
    {
        float3 vertex : POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct VaryingsDefault
    {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    SamplerState _LinearClamp;

    // TEXTURE2D_X(_OutlineGrabpass);
    // SAMPLER(sampler_OutlineGrabpass);

    TEXTURE2D_X(_OutlineSDF);
    SAMPLER(sampler_OutlineSDF);

    float4 _OutlineColor;
    float4 _OutlineSDF_TexelSize;
    float _MaximumOutlineDistanceInPixels;

    TEXTURE2D_X(_CorgiDepthGrabpassFullRes);
    SAMPLER(sampler_CorgiDepthGrabpassFullRes);

    TEXTURE2D_X(_CorgiDepthGrabpassNonFullRes);
    SAMPLER(sampler_CorgiDepthGrabpassNonFullRes);

    VaryingsDefault VertDefault(AttributesDefault v)
    {
        VaryingsDefault o;

        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

        o.vertex = float4(v.vertex.xy, 0.0, 1.0);
        o.uv = (v.vertex.xy + 1.0) * 0.5;

        #if UNITY_UV_STARTS_AT_TOP
                o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
        #endif

        return o;
    }

    float GetOutlineAlpha(float2 uv)
    {
        float4 sdf;

#ifdef DEPTH_AWARE_UPSAMPLE
        float hd0 = SAMPLE_TEXTURE2D_X(_CorgiDepthGrabpassFullRes, sampler_CorgiDepthGrabpassFullRes, uv).r;
        float dd0 = SAMPLE_TEXTURE2D_X_BIAS(_CorgiDepthGrabpassNonFullRes, sampler_CorgiDepthGrabpassNonFullRes, uv, int2(0, 1)).r;
        float dd1 = SAMPLE_TEXTURE2D_X_BIAS(_CorgiDepthGrabpassNonFullRes, sampler_CorgiDepthGrabpassNonFullRes, uv, int2(0, -1)).r;
        float dd2 = SAMPLE_TEXTURE2D_X_BIAS(_CorgiDepthGrabpassNonFullRes, sampler_CorgiDepthGrabpassNonFullRes, uv, int2(1, 0)).r;
        float dd3 = SAMPLE_TEXTURE2D_X_BIAS(_CorgiDepthGrabpassNonFullRes, sampler_CorgiDepthGrabpassNonFullRes, uv, int2(-1, 0)).r;

        float d0 = abs(hd0 - dd0);
        float d1 = abs(hd0 - dd1);
        float d2 = abs(hd0 - dd2);
        float d3 = abs(hd0 - dd3);

        float minD = min(min(d0, d1), min(d2, d3));

        if ( abs(minD - d0) < 0.0001)
        {
            sdf = SAMPLE_TEXTURE2D_X_BIAS(_OutlineSDF, sampler_OutlineSDF, uv, int2(0, 1));
        }
        else if ( abs(minD - d1) < 0.0001)
        {
            sdf = SAMPLE_TEXTURE2D_X_BIAS(_OutlineSDF, sampler_OutlineSDF, uv, int2(0, -1));
        }
        else if ( abs(minD - d2) < 0.0001)
        {
            sdf = SAMPLE_TEXTURE2D_X_BIAS(_OutlineSDF, sampler_OutlineSDF, uv, int2(1, 0));
        }
        else if ( abs(minD - d3) < 0.0001)
        {
            sdf = SAMPLE_TEXTURE2D_X_BIAS(_OutlineSDF, sampler_OutlineSDF, uv, int2(-1, 0));
        }
        else
        {
            sdf = SAMPLE_TEXTURE2D_X(_OutlineSDF, sampler_OutlineSDF, uv);
        }
#else
        sdf = SAMPLE_TEXTURE2D_X(_OutlineSDF, sampler_OutlineSDF, uv);
#endif


        // float4 sdf = SAMPLE_TEXTURE2D_X(_OutlineSDF, sampler_OutlineSDF, uv);

        sdf.w = saturate(pow(saturate(sdf.w), 2) * 100);

        float hueSlider = sdf.z * 64;
        float withinRange = sdf.z < _MaximumOutlineDistanceInPixels && sdf.z > -0.0001;
        return sdf.w * withinRange; 
    }

    float4 Frag(VaryingsDefault i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

#ifdef CORGI_QUALITY_HIGH_SAMPLING
        float2 sampleDistance = _OutlineSDF_TexelSize.xy;
        float alpha00 = GetOutlineAlpha(i.uv + float2(0, sampleDistance.y));
        float alpha01 = GetOutlineAlpha(i.uv - float2(0, sampleDistance.y));
        float alpha10 = GetOutlineAlpha(i.uv + float2(sampleDistance.x, 0));
        float alpha11 = GetOutlineAlpha(i.uv - float2(sampleDistance.x, 0));

        float alpha = (alpha00 + alpha01 + alpha10 + alpha11) * 0.25;
#else
        float alpha = GetOutlineAlpha(i.uv);
#endif

        // float4 col = SAMPLE_TEXTURE2D_X(_OutlineGrabpass, sampler_OutlineGrabpass, i.uv);
        // col.rgb = lerp(col.rgb, _OutlineColor.rgb, alpha * _OutlineColor.a);
        return float4(_OutlineColor.rgb, alpha * _OutlineColor.a);
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

        Blend SrcAlpha OneMinusSrcAlpha

        Cull Off 
        ZWrite Off 
        ZTest Always

        Pass
        {

            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag
                #pragma multi_compile_instancing
                #pragma multi_compile _ CORGI_QUALITY_HIGH_SAMPLING
                #pragma multi_compile _ DEPTH_AWARE_UPSAMPLE

            ENDHLSL
        }
    }
}
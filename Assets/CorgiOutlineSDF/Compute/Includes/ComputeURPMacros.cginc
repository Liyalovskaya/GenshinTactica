// from TextureXR.hlsl from HDRP 
// note: modified this a bit to work with URP (cannot assign unity_StereoEyeIndex?)
#if defined(CORGI_SPI_ENABLED)
    #if (defined(SHADER_API_D3D11) && !defined(SHADER_API_XBOXONE) && !defined(SHADER_API_GAMECORE)) || defined(SHADER_API_PSSL) || defined(SHADER_API_VULKAN)
        #define UNITY_TEXTURE2D_X_ARRAY_SUPPORTED
    #endif

    #if defined(UNITY_TEXTURE2D_X_ARRAY_SUPPORTED) && !defined(DISABLE_TEXTURE2D_X_ARRAY)
        #define USE_TEXTURE2D_X_AS_ARRAY
    #endif

    #if defined(STEREO_INSTANCING_ON) && defined(UNITY_TEXTURE2D_X_ARRAY_SUPPORTED)
        #define UNITY_STEREO_INSTANCING_ENABLED
    #endif

    #if defined(UNITY_TEXTURE2D_X_ARRAY_SUPPORTED) && (defined(SHADER_STAGE_COMPUTE) || defined(SHADER_STAGE_RAY_TRACING))
        #define UNITY_STEREO_INSTANCING_ENABLED
    #endif

    #if defined(UNITY_STEREO_INSTANCING_ENABLED)
        #define USING_STEREO_MATRICES
    #endif
#endif

#if defined(UNITY_STEREO_INSTANCING_ENABLED) 

    #define RW_TEXTURE2D_X(type, textureName)                                RW_TEXTURE2D_ARRAY(type, textureName)
    #define COORD_TEXTURE2D_X(pixelCoord)                                    uint3(pixelCoord, XR_VIEW_INDEX)
#else
    #define RW_TEXTURE2D_X                                                   RW_TEXTURE2D
    #define COORD_TEXTURE2D_X(pixelCoord)                                    pixelCoord
#endif

#if defined(UNITY_STEREO_INSTANCING_ENABLED)
    #define UNITY_XR_ASSIGN_VIEW_INDEX(viewIndex) uint XR_VIEW_INDEX = viewIndex
#else
    #define UNITY_XR_ASSIGN_VIEW_INDEX(viewIndex) uint XR_VIEW_INDEX = 0
#endif

#if defined(UNITY_STEREO_INSTANCING_ENABLED)
    #define INSTANCINGARG ,XR_VIEW_INDEX
#else
    #define INSTANCINGARG 
#endif

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
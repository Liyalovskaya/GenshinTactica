#include "ComputeURPMacros.cginc"

// R8 
TEXTURE2D_X(MaskInput);
RW_TEXTURE2D_X(half, MaskOutput);

// RGBA_Float
// xy = uv of surface, z = distance from surface, w = confidence (alpha) 
RW_TEXTURE2D_X(half4, SDFInput);
RW_TEXTURE2D_X(half4, SDFOutput);

uniform float texture_width;
uniform float texture_height;
uniform float maximum_outline_width;

bool OutOfBounds(int2 id)
{
    return id.x < 0 || id.x >= texture_width || id.y < 0 || id.y >= texture_height;
}

#define BOUNDS_CLIP(id) [branch] if(OutOfBounds(id)) return;

[numthreads(16,16,1)]
void ClearMask(int3 id : SV_DispatchThreadID)
{
    UNITY_XR_ASSIGN_VIEW_INDEX(id.z);
    BOUNDS_CLIP(id.xy)

    MaskOutput[COORD_TEXTURE2D_X(id.xy)] = 0.0;
}

[numthreads(16, 16, 1)]
void ClearSDF(int3 id : SV_DispatchThreadID)
{
    UNITY_XR_ASSIGN_VIEW_INDEX(id.z);
    BOUNDS_CLIP(id.xy)

    SDFOutput[COORD_TEXTURE2D_X(id.xy)] = float4(0.0, 0.0, 0.0, -1.0);
    // SDFOutput[COORD_TEXTURE2D_X(id.xy)] = 1;
}

[numthreads(16, 16, 1)]
void Initialize(int3 id : SV_DispatchThreadID)
{
    UNITY_XR_ASSIGN_VIEW_INDEX(id.z);
    BOUNDS_CLIP(id.xy)

    bool inside  = false;
    bool outside = false;
    
    [unroll] for (int x = -1; x <= 1; ++x)
    {
        [unroll] for (int y = -1; y <= 1; ++y)
        {
            int2 targetIndex = id.xy + int2(x, y);
            float input = MaskInput[COORD_TEXTURE2D_X(targetIndex)].x;
    
            [flatten] if (input > 0)
            {
                inside = true;
            }
            else 
            {
                outside = true; 
            }
        }
    }
    
    bool is_edge = inside && outside; 
    
    float uv_x = (float) id.x / texture_width;
    float uv_y = (float) id.y / texture_height;
    
    SDFOutput[COORD_TEXTURE2D_X(id.xy)] = float4(uv_x, uv_y, is_edge ? 0.0 : -1.0, 0.0);
}

[numthreads(16, 16, 1)]
void Step(int3 id : SV_DispatchThreadID)
{
    UNITY_XR_ASSIGN_VIEW_INDEX(id.z);
    BOUNDS_CLIP(id.xy)

    // bounds checks 
    [branch] if (id.x <= 1 || id.y <= 1 || id.x >= texture_width - 1 || id.y >= texture_height - 1)
    {
        SDFOutput[COORD_TEXTURE2D_X(id.xy)] = float4(0, 0, -1, 0);
        return;
    }

    float4 smallest_result = float4(0, 0, -1, 0);
    float smallest_distance = 100;

    float uv_x = (float) id.x / texture_width;
    float uv_y = (float) id.y / texture_height;
    float2 uv = float2(uv_x, uv_y);

    [unroll] for (int x = -1; x <= 1; ++x)
    {
        [unroll] for (int y = -1; y <= 1; ++y)
        {
            float4 result = SDFInput[COORD_TEXTURE2D_X(id.xy + int2(x, y))];
            float result_distance = result.z;

            if (result_distance >= 0)
            {
                float2 result_uv = result.xy;
                float2 to_result = result_uv - uv;
                float to_distance = length(to_result); 

                if (to_distance < smallest_distance)
                {
                    smallest_distance = to_distance;
                    smallest_result = result;
                }
            }
        }
    }

    // did not find any surfaces
    if (smallest_distance == 100.0)
    {
        SDFOutput[COORD_TEXTURE2D_X(id.xy)] = smallest_result;
    }
    // found a surface or a pointer to surfacce
    else 
    {
        float4 result = smallest_result;
        result.z = smallest_distance;
        result.w = 1.0 - smallest_distance; //  *texture_width / maximum_outline_width;
        SDFOutput[COORD_TEXTURE2D_X(id.xy)] = result;
    }
}

[numthreads(16, 16, 1)]
void SubtractMask(int3 id : SV_DispatchThreadID)
{
    UNITY_XR_ASSIGN_VIEW_INDEX(id.z);
    BOUNDS_CLIP(id.xy)

    float input_mask = MaskInput[COORD_TEXTURE2D_X(id.xy)].x;
    float4 input_sdf = SDFInput[COORD_TEXTURE2D_X(id.xy)];

    if (input_mask > 0)
    {
        SDFOutput[COORD_TEXTURE2D_X(id.xy)] = float4(input_sdf.xy, -1, 0);
    }
    else 
    {
        SDFOutput[COORD_TEXTURE2D_X(id.xy)] = input_sdf;
    }
}
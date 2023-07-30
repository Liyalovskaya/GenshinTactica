namespace CorgiOutlineSDF
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class RenderSettingsOutlineSDF
    {
        // config 
        [Tooltip("Max distance (in pixels) outlines will reach from their source object surfaces. Higher values will consume more GPU time.")] 
        [Range(1, 128)] public int MaximumOutlineDistanceInPixels = 1;
        
        [Tooltip("The layers used when creating the mask used for outlines. Keep in mind that anything with this layer will be rendered a second time, for this effect to work.")] 
        public LayerMask RenderLayers;
        
        [Tooltip("The color of the outline.")] 
        [ColorUsage(true, true)] public Color OutlineColor;
        
        [Tooltip("Enabling this allows the mask to re-use the depth buffer, so outlines do not go through walls. Disabling this makes the effect slightly cheaper.")] 
        public bool UseDepthTexture;
        
        [Tooltip("Raise this value to make the effect cheaper.")] 
        [Range(1, 4)] public int TextureDownscale = 1;

        [Tooltip("Only use this if absolutely necessary. Doubles the GPU memory usage and load.")] 
        public bool useHighQualityTextures = false;
        
        [Tooltip("Smooths out the outline effect as it's applied to the camera.")] 
        public bool useHighQualitySampling = true;

        [Tooltip("When using a lower texture quality, enabling this may make things look nicer. Only has an effect if not on the highest texture quality.")]
        public bool depthAwareUpsampling = false;

        [Tooltip("Offsets the render queue for this effect. Default is 0. Change if you are experiencing issues with the render order of this plugin with others.")]
        public int renderOrderOffset = 0;

        [Tooltip("If enabled, the outline mask shader will account for distances to allow for clipping outlines near and far from the camera.")]
        public bool OutlineUseNearFarClipPlanes;

        [Tooltip("If the object that would have an outline is closer than this, do not render it's outline.")]
        public float OutlineNearClipPlane = 0f;

        [Tooltip("If the object that would have an outline is farther than this, do not render it's outline.")]
        public float OutlineFarClipPlane = 1000f;
        
        // data 
        public RenderDataOutlineSDF data;
    }
}
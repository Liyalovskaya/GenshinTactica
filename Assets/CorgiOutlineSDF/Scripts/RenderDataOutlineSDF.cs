namespace CorgiOutlineSDF
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu( fileName = "RenderDataOutlineSDF", menuName = "Create/RenderDataOutlineSDF")]
    public class RenderDataOutlineSDF : ScriptableObject
    {
        public ComputeShader SDFComputeMP;
        public ComputeShader SDFComputeSPI;

        public Material ObjectMaterialOverride;
        public Material BlitMaterial;
        public Material GrabpassMaterial;
        public Material GrabpassDepthMaterial;
        public Material DepthGrabpass;
    }
}
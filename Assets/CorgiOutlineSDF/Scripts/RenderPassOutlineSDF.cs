namespace CorgiOutlineSDF
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using Unity.Mathematics;

    public class RenderPassOutlineSDF : ScriptableRenderPass
    {
        private const string _profilerTag = "RenderPassOutlineSDF";
        [System.NonSerialized] private RenderSettingsOutlineSDF settings;
        [System.NonSerialized] private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        [System.NonSerialized] private FilteringSettings m_FilteringSettings;
        [System.NonSerialized] private RenderStateBlock m_RenderStateBlock;
        [System.NonSerialized] private ScriptableRenderer _renderer;

        // internal 
        [System.NonSerialized] private int kernal_ClearMask;
        [System.NonSerialized] private int kernal_ClearSDF;
        [System.NonSerialized] private int kernal_Initialize;
        [System.NonSerialized] private int kernal_Step;
        [System.NonSerialized] private int kernal_SubtractMask;
        [System.NonSerialized] private int compute_id_MaskInput;
        [System.NonSerialized] private int compute_id_MaskOutput;
        [System.NonSerialized] private int compute_id_SDFInput;
        [System.NonSerialized] private int compute_id_SDFOutput;
        [System.NonSerialized] private int compute_id_texture_width;
        [System.NonSerialized] private int compute_id_texture_height;
        [System.NonSerialized] private int compute_id_maximum_outline_width;
        [System.NonSerialized] private int maskRenderId = Shader.PropertyToID("MaskRender");
        [System.NonSerialized] private int maskComputeId = Shader.PropertyToID("MaskCompute");
        [System.NonSerialized] private int sdfPairA = Shader.PropertyToID("SDF_A");
        [System.NonSerialized] private int sdfPairB = Shader.PropertyToID("SDF_B");

        private static readonly int _CopyBlitTex = Shader.PropertyToID("_CopyBlitTex");
        private static readonly int _CorgiDepthGrabpassFullRes = Shader.PropertyToID("_CorgiDepthGrabpassFullRes");
        private static readonly int _CorgiDepthGrabpassNonFullRes = Shader.PropertyToID("_CorgiDepthGrabpassNonFullRes");

        private static readonly int _CorgiOutlineNearClipPlane = Shader.PropertyToID("_CorgiOutlineNearClipPlane");
        private static readonly int _CorgiOutlineFarClipPlane = Shader.PropertyToID("_CorgiOutlineFarClipPlane");

        private LocalKeyword _CORGIOUTLINE_USENEARFARCLIP;
        private bool _initializedLocalKeyword; 

        public void Setup(RenderSettingsOutlineSDF settings, ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            this.settings = settings;
            this._renderer = renderer;

            renderPassEvent = RenderPassEvent.AfterRenderingSkybox + settings.renderOrderOffset;
            
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, settings.RenderLayers);
            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

            kernal_ClearMask = settings.data.SDFComputeMP.FindKernel("ClearMask");
            kernal_ClearSDF = settings.data.SDFComputeMP.FindKernel("ClearSDF");
            kernal_Initialize = settings.data.SDFComputeMP.FindKernel("Initialize");
            kernal_Step = settings.data.SDFComputeMP.FindKernel("Step");
            kernal_SubtractMask = settings.data.SDFComputeMP.FindKernel("SubtractMask");

            compute_id_MaskInput = Shader.PropertyToID("MaskInput");
            compute_id_MaskOutput = Shader.PropertyToID("MaskOutput");
            compute_id_SDFInput = Shader.PropertyToID("SDFInput");
            compute_id_SDFOutput = Shader.PropertyToID("SDFOutput");
            compute_id_texture_width = Shader.PropertyToID("texture_width");
            compute_id_texture_height = Shader.PropertyToID("texture_height");
            compute_id_maximum_outline_width = Shader.PropertyToID("maximum_outline_width");
            
            if(settings.UseDepthTexture)
            {
                ConfigureInput(ScriptableRenderPassInput.Depth);
            }
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!_initializedLocalKeyword)
            {
                _initializedLocalKeyword = true;
            }

            _CORGIOUTLINE_USENEARFARCLIP = new LocalKeyword(settings.data.ObjectMaterialOverride.shader, "_CORGIOUTLINE_USENEARFARCLIP");

            // setup 
            if (m_ShaderTagIdList == null || m_ShaderTagIdList.Count == 0)
            {
                m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
                m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
                m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
            }

            // version compatibility 
#if UNITY_2022_1_OR_NEWER
            var cameraColorTarget = _renderer.cameraColorTargetHandle;
            var cameraDepthTarget = _renderer.cameraDepthTargetHandle;
#else
            var cameraColorTarget = _renderer.cameraColorTarget;
            var cameraDepthTarget = _renderer.cameraDepthTarget;
#endif

            // command start 
            var cmd = CommandBufferPool.Get(_profilerTag);
                cmd.Clear();


            GetMaskAndSDFHandles(cmd, renderingData, out int maskRender, out int maskCompute, out RenderTextureHandlePair sdfPair);

            // depth grabpasses
            var depthAwareUpscaling = settings.depthAwareUpsampling && settings.TextureDownscale > 1 && settings.UseDepthTexture;
            if (depthAwareUpscaling)
            {
                // depth grabpass  (full res)
                var colorTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                var depthTargetDescriptorFullRes = colorTargetDescriptor;
                    depthTargetDescriptorFullRes.colorFormat = RenderTextureFormat.RFloat;

                cmd.SetGlobalTexture(_CopyBlitTex, cameraDepthTarget);
                cmd.GetTemporaryRT(_CorgiDepthGrabpassFullRes, depthTargetDescriptorFullRes);
                cmd.SetRenderTarget(_CorgiDepthGrabpassFullRes, 0, CubemapFace.Unknown, -1);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, settings.data.DepthGrabpass, 0, 0);
                cmd.SetGlobalTexture(_CorgiDepthGrabpassFullRes, _CorgiDepthGrabpassFullRes);

                // depth grabpass (half res) 
                var depthTargetDescriptorNonFullRes = renderingData.cameraData.cameraTargetDescriptor;
                    depthTargetDescriptorNonFullRes.colorFormat = RenderTextureFormat.RFloat;
                    depthTargetDescriptorNonFullRes.width = sdfPair.GetWidth();
                    depthTargetDescriptorNonFullRes.height = sdfPair.GetHeight();

                cmd.SetGlobalTexture(_CopyBlitTex, cameraDepthTarget);
                cmd.GetTemporaryRT(_CorgiDepthGrabpassNonFullRes, depthTargetDescriptorNonFullRes);
                cmd.SetRenderTarget(_CorgiDepthGrabpassNonFullRes, 0, CubemapFace.Unknown, -1);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, settings.data.DepthGrabpass, 0, 0);
            }

            var useDepth = settings.UseDepthTexture;
            if(useDepth)
            {
                cmd.SetRenderTarget(maskRender, cameraDepthTarget, 0, CubemapFace.Unknown, -1);
            }
            else
            {
                cmd.SetRenderTarget(maskRender, maskRender, 0, CubemapFace.Unknown, -1);
            }

            // setup keywords 
            if (settings.OutlineUseNearFarClipPlanes)
            {
                // note: if you're running low on global keywords, you can swap out these two lines 
                // the downside is that you will not be able to use different keyword values here for each render pass, if you have multiple outlines 

                // settings.data.ObjectMaterialOverride.EnableKeyword(_CORGIOUTLINE_USENEARFARCLIP);
                cmd.EnableShaderKeyword("_CORGIOUTLINE_USENEARFARCLIP");

                cmd.SetGlobalFloat(_CorgiOutlineNearClipPlane, settings.OutlineNearClipPlane);
                cmd.SetGlobalFloat(_CorgiOutlineFarClipPlane, settings.OutlineFarClipPlane);
            }
            else
            {
                // note: if you're running low on global keywords, you can swap out these two lines 
                // the downside is that you will not be able to use different keyword values here for each render pass, if you have multiple outlines 

                // settings.data.ObjectMaterialOverride.DisableKeyword(_CORGIOUTLINE_USENEARFARCLIP);
                cmd.DisableShaderKeyword("_CORGIOUTLINE_USENEARFARCLIP");
            }

            cmd.ClearRenderTarget(false, true, new Color(0f, 0f, 0f, 0f));
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // draw the renderers into this render target using the special material (for the mask) 
            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = settings.data.ObjectMaterialOverride;
            drawingSettings.overrideMaterialPassIndex = 0;
            
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings,
                ref m_RenderStateBlock);
            
            // run the outline sdf steps on this texture
            cmd.Clear();

            // blit the mask render into a compute-friendly texture 
            // note: if we're not using depth, this is unnecessary 
            if(useDepth || renderingData.cameraData.cameraTargetDescriptor.msaaSamples > 1)
            {
                cmd.SetGlobalTexture("_CopyBlitTex", maskRender);
                cmd.SetRenderTarget(maskCompute, 0, CubemapFace.Unknown, -1);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, settings.data.GrabpassMaterial, 0, 0);
            }

            RenderMaskAndSDF(cmd, maskCompute, ref sdfPair);

            cmd.SetGlobalTexture("_OutlineSDF", sdfPair.GetReader());
            cmd.SetGlobalFloat("_MaximumOutlineDistanceInPixels", settings.MaximumOutlineDistanceInPixels);
            cmd.SetGlobalColor("_OutlineColor", settings.OutlineColor);
            
            // blit this texture to the screen 
            if(settings.useHighQualitySampling)
            {
                cmd.EnableShaderKeyword("CORGI_QUALITY_HIGH_SAMPLING");
            }
            else
            {
                cmd.DisableShaderKeyword("CORGI_QUALITY_HIGH_SAMPLING");
            }

            if (depthAwareUpscaling)
            {
                cmd.SetGlobalTexture(_CorgiDepthGrabpassNonFullRes, _CorgiDepthGrabpassNonFullRes);
                cmd.EnableShaderKeyword("DEPTH_AWARE_UPSAMPLE");
            }
            else
            {
                cmd.DisableShaderKeyword("DEPTH_AWARE_UPSAMPLE");
            }

            cmd.SetRenderTarget(cameraColorTarget, useDepth ? cameraDepthTarget : cameraColorTarget, 0, CubemapFace.Unknown, -1);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, settings.data.BlitMaterial, 0, 0);

            cmd.ReleaseTemporaryRT(maskRender);
            cmd.ReleaseTemporaryRT(maskCompute);
            cmd.ReleaseTemporaryRT(sdfPair.GetReader());
            cmd.ReleaseTemporaryRT(sdfPair.GetWriter());

            // finish up 
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void GetMaskAndSDFHandles(CommandBuffer command, RenderingData renderingData, out int maskRender, out int maskCompute, out RenderTextureHandlePair sdfPair)
        {
            var targetDesc = renderingData.cameraData.cameraTargetDescriptor;

            // var size = Mathf.Min(Mathf.ClosestPowerOfTwo(targetDesc.width), Mathf.ClosestPowerOfTwo(targetDesc.height));
            //     size /= settings.TextureDownscale;
            // 
            // targetDesc.width = size;
            // targetDesc.height = size;

            targetDesc.width /= settings.TextureDownscale;
            targetDesc.height /= settings.TextureDownscale;

            var sdfDesc = targetDesc;
                sdfDesc.colorFormat = settings.useHighQualityTextures ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.ARGBHalf;
                sdfDesc.depthBufferBits = 0;
                sdfDesc.msaaSamples = 1;
                sdfDesc.bindMS = false;
                sdfDesc.enableRandomWrite = true;
                sdfDesc.autoGenerateMips = false;
                sdfDesc.mipCount = 1;

            sdfPair = new RenderTextureHandlePair(sdfPairA, sdfPairB, sdfDesc);
            sdfPair.AddCommands(command);

            // the mask texture gets blit/scaled into a power of 2 (compute friendly) texture 
            var maskComputeDesc = targetDesc;
                maskComputeDesc.colorFormat = RenderTextureFormat.R8;
                maskComputeDesc.depthBufferBits = 0;
                maskComputeDesc.msaaSamples = 1;
                maskComputeDesc.bindMS = false;
                maskComputeDesc.enableRandomWrite = true;
                maskComputeDesc.autoGenerateMips = false;
                maskComputeDesc.mipCount = 1;

            maskCompute = maskComputeId;
            command.GetTemporaryRT(maskCompute, maskComputeDesc);

            if(!settings.UseDepthTexture && renderingData.cameraData.cameraTargetDescriptor.msaaSamples == 1)
            {
                maskRender = maskComputeId;
                return;
            }

            // rendering into a full-res mask texture 
            var maskRenderDesc = renderingData.cameraData.cameraTargetDescriptor;
                maskRenderDesc.colorFormat = RenderTextureFormat.R8;
                maskRenderDesc.depthBufferBits = 0;
                maskRenderDesc.autoGenerateMips = false;
                maskRenderDesc.mipCount = 1;

            maskRender = maskRenderId;
            command.GetTemporaryRT(maskRender, maskRenderDesc);
        }

        public void RenderMaskAndSDF(CommandBuffer command, int maskHandle, ref RenderTextureHandlePair sdfPair)
        {
            var compute = sdfPair.GetVolume() > 1 ? settings.data.SDFComputeSPI : settings.data.SDFComputeMP;

            command.SetComputeFloatParam(compute, compute_id_texture_width, sdfPair.GetWidth());
            command.SetComputeFloatParam(compute, compute_id_texture_height, sdfPair.GetHeight());
            command.SetComputeFloatParam(compute, compute_id_maximum_outline_width, settings.MaximumOutlineDistanceInPixels);

            InitializeSDFFromMask(command, maskHandle, ref sdfPair);
            
            for (var i = 0; i < settings.MaximumOutlineDistanceInPixels; ++i)
            {
                StepSDF(command, maskHandle, ref sdfPair);
            }
            
            SubtractMaskFromSDF(command, maskHandle, ref sdfPair);
        }

        private int3 GetDispatchSizes(int2 textureDimensions, int3 threadGroup, int volumeDepth)
        {
            var dispatch_x = textureDimensions.x / threadGroup.x;
            var dispatch_y = textureDimensions.y / threadGroup.y;
            var dispatch_z = volumeDepth;

            while (dispatch_x * threadGroup.x < textureDimensions.x) dispatch_x += 1;
            while (dispatch_y * threadGroup.y < textureDimensions.y) dispatch_y += 1;

            return new int3(dispatch_x, dispatch_y, dispatch_z);
        }

        private void InitializeSDFFromMask(CommandBuffer command, int maskHandle, ref RenderTextureHandlePair sdfPair)
        {
            var volumeDepth = sdfPair.GetVolume();
            var compute = volumeDepth > 1 ? settings.data.SDFComputeSPI : settings.data.SDFComputeMP;
                compute.GetKernelThreadGroupSizes(kernal_Initialize, out uint threadGroupX, out uint threadGroupY, out uint threadGroupZ);

            var dispatches = GetDispatchSizes(new int2(sdfPair.GetWidth(), sdfPair.GetHeight()), new int3((int) threadGroupX, (int)threadGroupY, (int) threadGroupZ), volumeDepth);

            command.SetComputeTextureParam(compute, kernal_Initialize, compute_id_MaskInput, maskHandle, 0);
            command.SetComputeTextureParam(compute, kernal_Initialize, compute_id_SDFOutput, sdfPair.GetWriter(), 0, RenderTextureSubElement.Color);
            command.DispatchCompute(compute, kernal_Initialize, dispatches.x, dispatches.y, dispatches.z);

            sdfPair.Swap();
        }

        private void SubtractMaskFromSDF(CommandBuffer command, int maskHandle, ref RenderTextureHandlePair sdfPair)
        {
            var volumeDepth = sdfPair.GetVolume(); 
            var compute = volumeDepth > 1 ? settings.data.SDFComputeSPI : settings.data.SDFComputeMP;
                compute.GetKernelThreadGroupSizes(kernal_SubtractMask, out uint threadGroupX, out uint threadGroupY, out uint threadGroupZ);

            var dispatches = GetDispatchSizes(new int2(sdfPair.GetWidth(), sdfPair.GetHeight()), new int3((int)threadGroupX, (int)threadGroupY, (int)threadGroupZ), volumeDepth);

            command.SetComputeTextureParam(compute, kernal_SubtractMask, compute_id_MaskInput, maskHandle);
            command.SetComputeTextureParam(compute, kernal_SubtractMask, compute_id_SDFInput, sdfPair.GetReader());
            command.SetComputeTextureParam(compute, kernal_SubtractMask, compute_id_SDFOutput, sdfPair.GetWriter(), 0);
            command.DispatchCompute(compute, kernal_SubtractMask, dispatches.x, dispatches.y, dispatches.z);

            sdfPair.Swap();
        }

        private void StepSDF(CommandBuffer command, int maskHandle, ref RenderTextureHandlePair sdfPair)
        {
            var volumeDepth = sdfPair.GetVolume();
            var compute = volumeDepth > 1 ? settings.data.SDFComputeSPI : settings.data.SDFComputeMP;
                compute.GetKernelThreadGroupSizes(kernal_Step, out uint threadGroupX, out uint threadGroupY, out uint threadGroupZ);

            var dispatches = GetDispatchSizes(new int2(sdfPair.GetWidth(), sdfPair.GetHeight()), new int3((int)threadGroupX, (int)threadGroupY, (int)threadGroupZ), volumeDepth);

            command.SetComputeTextureParam(compute, kernal_Step, compute_id_SDFInput, sdfPair.GetReader());
            command.SetComputeTextureParam(compute, kernal_Step, compute_id_SDFOutput, sdfPair.GetWriter(), 0);
            command.DispatchCompute(compute, kernal_Step, dispatches.x, dispatches.y, dispatches.z);

            sdfPair.Swap();
        }
    }
}
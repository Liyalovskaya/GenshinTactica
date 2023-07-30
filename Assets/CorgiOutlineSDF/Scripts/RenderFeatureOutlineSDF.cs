namespace CorgiOutlineSDF
{

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;

    public class RenderFeatureOutlineSDF : ScriptableRendererFeature
    {
        public RenderSettingsOutlineSDF settings = new RenderSettingsOutlineSDF();
        private RenderPassOutlineSDF _renderPass;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(settings.data == null)
            {
                return;
            }

            _renderPass.Setup(settings, renderer, ref renderingData);

            renderer.EnqueuePass(_renderPass);
        }

        public override void Create()
        {
            _renderPass = new RenderPassOutlineSDF();
        }
    }
}
using System;
using System.Threading.Tasks;
using DG.Tweening;
using GT.Core;
using UnityEngine;
using UnityEngine.Serialization;
using Cysharp.Threading.Tasks;

namespace GT.Presentation
{
    public class Indicator : MonoBehaviour
    {
        public BattleGrid BattleGrid;
        public MeshRenderer indRenderer, liftRenderer;
        [SerializeField] private Texture defaultTexture;
        [SerializeField] private Texture targetTexture;
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");

        private Material _indMaterial, _liftMaterial;
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        public bool setTarget = false;

        private void Awake()
        {
            _indMaterial = indRenderer.material;
            _liftMaterial = liftRenderer.material;

        }

        public void OnSelected()
        {
            _indMaterial.SetFloat(Alpha,1f);
            _indMaterial.SetTexture(MainTex, targetTexture);
            liftRenderer.enabled = true;
            _liftMaterial.SetFloat(Alpha, 1f);
        }

        public void OnDeselected()
        {
            _indMaterial.SetFloat(Alpha,.25f);
            _indMaterial.SetTexture(MainTex, defaultTexture);
            liftRenderer.enabled = false;
            _liftMaterial.SetFloat(Alpha, 0f);
        }

        public void SetTarget()
        {
            setTarget = true;
        }

        public async Task ReachTarget()
        {
            setTarget = false;
            var t = .2f;
            DOVirtual.Float(1f, 0f, t, x =>
            {
                _indMaterial.SetFloat(Alpha, x);
                _liftMaterial.SetFloat(Alpha, x);

            }).SetEase(Ease.OutCubic);
            await UniTask.WaitForSeconds(t);
            liftRenderer.enabled = false;
            gameObject.SetActive(false);
        }

        // public void SetOutline(int bit)
        // {
        //     if (bit == 0)
        //     {
        //         outlineRenderer.gameObject.SetActive(false);
        //         return;
        //     }
        //
        //     if (bit == 2)
        //     {
        //         _outlineMaterial.SetInt();
        //     }
        // }
    }
}

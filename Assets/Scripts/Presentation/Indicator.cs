using System;
using GT.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace GT.Presentation
{
    public class Indicator : MonoBehaviour
    {
        public BattleGrid BattleGrid;
        public MeshRenderer indRenderer;
        [SerializeField] private Texture defaultTexture;
        [SerializeField] private Texture targetTexture;
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");

        private Material _indMaterial;
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        private void Awake()
        {
            indRenderer = GetComponentInChildren<MeshRenderer>();
            _indMaterial = indRenderer.material;
        }

        public void OnSelected()
        {
            _indMaterial.SetFloat(Alpha,1f);
            _indMaterial.SetTexture(MainTex, targetTexture);
        }

        public void OnDeselected()
        {
            _indMaterial.SetFloat(Alpha,.25f);
            _indMaterial.SetTexture(MainTex, defaultTexture);
        }

        public void SetTarget()
        {
            
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

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

        [SerializeField] [ColorUsage(false, true)]
        private Color[] colors;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public IndicatorState State
        {
            set
            {
                switch (value)
                {
                    case IndicatorState.Default:
                        indRenderer.enabled = true;
                        _indMaterial.SetFloat(Alpha, .25f);
                        _indMaterial.SetTexture(MainTex, defaultTexture);
                        liftRenderer.enabled = false;
                        break;
                    case IndicatorState.Selected:
                        indRenderer.enabled = true;
                        _indMaterial.SetFloat(Alpha, 1f);
                        _indMaterial.SetTexture(MainTex, targetTexture);
                        liftRenderer.enabled = true;
                        _liftMaterial.SetFloat(Alpha, 1f);
                        break;
                }
            }
        }

        public IndicatorColor Color
        {
            set => _indMaterial.SetColor(BaseColor, colors[(int)value]);
        }

        private void Awake()
        {
            _indMaterial = indRenderer.material;
            _liftMaterial = liftRenderer.material;
        }

        public void SetTarget()
        {
            setTarget = true;
        }

        public void ReachTarget()
        {
            setTarget = false;
            liftRenderer.enabled = false;
            State = IndicatorState.Default;
            BattleMapManager.Instance.RefreshIndicator(BattleGrid);
        }
    }

    public enum IndicatorState
    {
        Default,
        Selected,
    }

    public enum IndicatorColor
    {
        Default,
        Player,
        Ally,
        Enemy,
    }
}
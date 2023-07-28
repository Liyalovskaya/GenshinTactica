using System;
using GT.Core;
using UnityEngine;

namespace GT.Presentation
{
    public class Indicator : MonoBehaviour
    {
        public BattleGrid BattleGrid;
        public MeshRenderer indRenderer;
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");

        private void Awake()
        {
            indRenderer = GetComponentInChildren<MeshRenderer>();
        }

        public void OnSelected()
        {
            indRenderer.material.SetFloat(Alpha,1f);
        }

        public void OnDeselected()
        {
            indRenderer.material.SetFloat(Alpha,.25f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using GT.DevTool;
using GT.Presentation;
using UnityEngine;
using UnityEditor;

namespace GT.Editor
{
    [CustomEditor(typeof(BattleMapBuilder))]
    public class BattleMapBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("GenerateMap"))
            {
                var builder = (BattleMapBuilder)target; 
                builder.BuildMap();

            }
        }
    }
}

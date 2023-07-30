namespace CorgiOutlineSDF
{
#if UNITY_EDITOR
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.UIElements;

    [CustomPropertyDrawer(typeof(RenderSettingsOutlineSDF))]
    public class RenderSEttingsOutlineSDFEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // draw default 
            // EditorGUI.PropertyField(position, property, label, true);

            if(!SystemInfo.supportsComputeShaders)
            {
                EditorGUILayout.HelpBox("Corgi Outlines requires ComputeShaders to function, but the current platform you are using does not support Compute Shaders.", MessageType.Error);
            }

            // find all child properties 
            var RenderLayers = property.FindPropertyRelative("RenderLayers");
            var OutlineColor = property.FindPropertyRelative("OutlineColor");
            var MaximumOutlineDistanceInPixels = property.FindPropertyRelative("MaximumOutlineDistanceInPixels");
            var UseDepthTexture = property.FindPropertyRelative("UseDepthTexture");
            var TextureDownscale = property.FindPropertyRelative("TextureDownscale");
            var useHighQualityTextures = property.FindPropertyRelative("useHighQualityTextures");
            var useHighQualitySampling = property.FindPropertyRelative("useHighQualitySampling");
            var depthAwareUpsampling = property.FindPropertyRelative("depthAwareUpsampling");
            var renderOrderOffset = property.FindPropertyRelative("renderOrderOffset");
            var OutlineUseNearFarClipPlanes = property.FindPropertyRelative("OutlineUseNearFarClipPlanes");
            var OutlineNearClipPlane = property.FindPropertyRelative("OutlineNearClipPlane");
            var OutlineFarClipPlane = property.FindPropertyRelative("OutlineFarClipPlane");

            var data = property.FindPropertyRelative("data");

            // settings 
            EditorGUILayout.BeginVertical("GroupBox");
            {
                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(RenderLayers); 
                EditorGUILayout.PropertyField(OutlineColor); 
                EditorGUILayout.PropertyField(MaximumOutlineDistanceInPixels); 
                EditorGUILayout.PropertyField(TextureDownscale);
                EditorGUILayout.PropertyField(UseDepthTexture);
                EditorGUILayout.PropertyField(useHighQualityTextures);
                EditorGUILayout.PropertyField(useHighQualitySampling);

                var doNotUseDepthAwareUpsampling = TextureDownscale.intValue <= 1 || !UseDepthTexture.boolValue;
                {
                    EditorGUI.BeginDisabledGroup(doNotUseDepthAwareUpsampling);
                    EditorGUILayout.PropertyField(depthAwareUpsampling);
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUILayout.PropertyField(OutlineUseNearFarClipPlanes);

                if(OutlineUseNearFarClipPlanes.boolValue)
                {
                    EditorGUILayout.PropertyField(OutlineNearClipPlane);
                    EditorGUILayout.PropertyField(OutlineFarClipPlane);
                }

                EditorGUILayout.PropertyField(renderOrderOffset);
            }
            EditorGUILayout.EndVertical();

            // data 
            EditorGUILayout.BeginVertical("GroupBox");
            {
                EditorGUILayout.LabelField("Data", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(data);
            }
            EditorGUILayout.EndVertical();

            // if data is null, slot in one from the project if we can find it 
            var dirty = false;

            if(data.objectReferenceValue == null)
            {
                var guids = AssetDatabase.FindAssets("t:RenderDataOutlineSDF"); 
                foreach(var guid in guids)
                {
                    if (string.IsNullOrEmpty(guid)) continue;

                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(path)) continue;

                    var renderData = AssetDatabase.LoadAssetAtPath<RenderDataOutlineSDF>(path);
                    data.objectReferenceValue = renderData;

                    Debug.LogWarning($"[Corgi Outlines]: RenderDataOutlineSDF data block was null, so one was automatically added. " +
                        $"It can be changed at anytime, but cannot be null.");

                    dirty = true;
                    break; 
                }
            }

            if (GUI.changed || dirty)
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return base.CreatePropertyGUI(property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }

#endif
}
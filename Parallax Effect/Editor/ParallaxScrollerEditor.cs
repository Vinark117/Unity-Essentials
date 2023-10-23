using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VinTools.MonoBehaviours.Editor
{
    /// <summary>
    /// An editor script for the parallax scroller script.
    /// Hides options if they're not used in the script.
    /// Adds a handle to set the custom reference position.
    /// </summary>
    /// 
    /// Script by Vinark
    [CustomEditor(typeof(ParallaxScroller))]
    public class ParallaxScrollerEditor : UnityEditor.Editor
    {
        private ParallaxScroller _parallax;

        private void OnEnable()
        {
            _parallax = target as ParallaxScroller;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.LabelField("Parallax behaviour", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ReferenceObject"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AffectAxes"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ScrollBehaviour"), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Auto scale", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AdaptiveParallaxScale"), true);
            if (_parallax.AdaptiveParallaxScale)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HorizonDistance"), true);
            }

            if (_parallax.ScrollBehaviour == ParallaxScroller.ParallaxScrollBehaviour.Absolute)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Absolute settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CustomReferencePosition"), true);
                if (_parallax.CustomReferencePosition)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("StartReference"), true);
                    EditorGUILayout.Space();
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("SnapToPixelGrid"), true);
                if (_parallax.SnapToPixelGrid)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PixelsPerUnit"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PixelOffset"), true);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Layers"), true);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (_parallax.ScrollBehaviour == ParallaxScroller.ParallaxScrollBehaviour.Absolute && _parallax.CustomReferencePosition)
            {
                Handles.color = new Color(1, .5f, .5f);
                
                Handles.DrawSolidDisc(_parallax.StartReference, Vector3.forward, .5f);
                _parallax.StartReference = Handles.FreeMoveHandle(_parallax.StartReference, .5f, Vector3.zero, Handles.CircleHandleCap);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace PupSurvivors.Stage.UI.LevelUp
{
    [CustomEditor(typeof(RewardPanel))]
    public class RewardPanelEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#if UNITY_EDITOR

#endif
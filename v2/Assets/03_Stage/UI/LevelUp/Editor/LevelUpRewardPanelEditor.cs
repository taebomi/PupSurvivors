using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Stage.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace PupSurvivors.Stage
{
    [CustomEditor(typeof(LevelUpRewardPanel))]
    public class LevelUpRewardPanelEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
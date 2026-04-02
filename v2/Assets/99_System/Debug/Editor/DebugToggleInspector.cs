using UnityEditor;
using UnityEditor.UI;

#if UNITY_EDITOR
namespace PupSurvivors.System.Debug
{
    [CustomEditor(typeof(DebugToggle))]
    public class DebugToggleInspector : ToggleEditor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif
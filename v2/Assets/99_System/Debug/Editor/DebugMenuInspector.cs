using PupSurvivors.System.Debug;
using UnityEditor;

namespace PupSurvivors.Debug
{
    [CustomEditor(typeof(DebugMenu))]
    public class DebugMenuInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
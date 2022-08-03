#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using UnityEngine;
public class SilantroDataLogger : MonoBehaviour
{

}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroDataLogger))]
public class SilantroDataLoggerEditor : Editor
{
    Color backgroundColor;
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Deprecated, please remove", MessageType.Warning);
        GUI.color = backgroundColor;
    }
}
#endif
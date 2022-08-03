#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
public class SilantroNozzle : MonoBehaviour
{
    // ----------------------------- Functionality has been moved, please remove
}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroNozzle))]
public class SilantroNozzleEditor : Editor
{
    Color backgroundColor;
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Functionality has been moved, please remove", MessageType.Warning);
        GUI.color = backgroundColor;
    }
}
#endif
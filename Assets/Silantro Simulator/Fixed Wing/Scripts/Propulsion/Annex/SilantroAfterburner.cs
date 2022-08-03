#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
public class SilantroAfterburner : MonoBehaviour
{
    // ----------------------------- Functionality has been moved, please remove
}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroAfterburner))]
public class SilantroAfterburnerEditor : Editor
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
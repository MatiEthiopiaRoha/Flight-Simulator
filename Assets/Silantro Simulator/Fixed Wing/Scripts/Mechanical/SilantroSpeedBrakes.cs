#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
public class SilantroSpeedBrakes : MonoBehaviour
{
    // ----------------------------- Functionality has been moved to , please remove
}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroSpeedBrakes))]
public class SilantroSpeedBrakesEditor : Editor
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
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
public class SilantroBlade : MonoBehaviour
{
    // ----------------------------- Functionality has been moved to "Silantro Propeller", please remove
}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroBlade))]
public class SilantroBladeEditor : Editor
{
    Color backgroundColor;
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Functionality has been moved to 'Silantro Propeller', please remove", MessageType.Warning);
        GUI.color = backgroundColor;
    }
}
#endif
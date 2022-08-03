#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
public class SilantroAirstripLights : MonoBehaviour
{

}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroAirstripLights))]
public class SilantroAirstripLightsEditor : Editor
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
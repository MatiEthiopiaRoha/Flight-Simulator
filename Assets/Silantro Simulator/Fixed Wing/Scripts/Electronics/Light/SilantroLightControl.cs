
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
public class SilantroLightControl : MonoBehaviour
{

}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroLightControl))]
public class SilantroLightControlEditor : Editor
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
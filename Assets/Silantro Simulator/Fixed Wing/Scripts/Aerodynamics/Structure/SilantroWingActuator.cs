using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SilantroWingActuator : MonoBehaviour
{
    // ----------------------------- Functionality has been moved, please remove
}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroWingActuator))]
public class SilantroWingActuatorEditor : Editor
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
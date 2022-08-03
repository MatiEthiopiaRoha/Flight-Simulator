using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SilantroFuelDistributor : MonoBehaviour
{

}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroFuelDistributor))]
public class SilantroFuelDistributorEditor : Editor
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
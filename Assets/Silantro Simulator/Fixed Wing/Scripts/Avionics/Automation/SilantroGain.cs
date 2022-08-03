using UnityEngine;
using Oyedoyin;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class SilantroGain : ScriptableObject
{
    public string label;

    // --------------------------------- Base Gains
    public Gain throttleGain;
    public Gain rollRateGain;
    public Gain pitchRateGain;
    public Gain yawRateGain;

    // ---------------------------------
    public Gain rollAngleGain;
    public Gain pitchAngleGain;
    public Gain yawAngleGain;

    // ---------------------------------
    public Gain altitudeGain;
    public Gain climbGain;
    public Gain turnGain;


    // --------------------------------- Advanced Gains





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeGains()
    {
        throttleGain = new Gain();
        rollRateGain = new Gain();
        pitchRateGain = new Gain();
        yawRateGain = new Gain();

        rollAngleGain = new Gain();
        pitchAngleGain = new Gain();
        yawAngleGain = new Gain();

        altitudeGain = new Gain();
        climbGain = new Gain();
        turnGain = new Gain();
    }
}






#if UNITY_EDITOR
[CustomEditor(typeof(SilantroGain))]
public class SilantroGainEditor : Editor
{
    Color backgroundColor;
    Color silantroColor = new Color(1, 0.4f, 0);


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        serializedObject.Update();
        SilantroGain gain = (SilantroGain)target;
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Label", gain.name);

        GUILayout.Space(8f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Inner Loop Gains", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);



        EditorGUI.indentLevel++;
        GUILayout.Space(10f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Throttle Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("throttleGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("throttleGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("throttleGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));

        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("throttleGain").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("throttleGain").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
        EditorGUI.indentLevel--;


        EditorGUI.indentLevel++;
        GUILayout.Space(10f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Roll Rate Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rollRateGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rollRateGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rollRateGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));

        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rollRateGain").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rollRateGain").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
        EditorGUI.indentLevel--;


        EditorGUI.indentLevel++;
        GUILayout.Space(10f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Pitch Rate Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchRateGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchRateGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchRateGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchRateGain").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchRateGain").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
        EditorGUI.indentLevel--;


        EditorGUI.indentLevel++;
        GUILayout.Space(10f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Yaw Rate Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("yawRateGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("yawRateGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("yawRateGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("yawRateGain").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("yawRateGain").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
        EditorGUI.indentLevel--;



        GUILayout.Space(15f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Outer Loop Gains", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);

        EditorGUI.indentLevel++;
        GUILayout.Space(10f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Pitch Hold Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchAngleGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchAngleGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchAngleGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchAngleGain").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchAngleGain").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
        EditorGUI.indentLevel--;


        EditorGUI.indentLevel++;
        GUILayout.Space(10f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Bank Hold Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rollAngleGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rollAngleGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rollAngleGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rollAngleGain").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rollAngleGain").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
        EditorGUI.indentLevel--;

        EditorGUI.indentLevel++;
        GUILayout.Space(10f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Heading Hold Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("yawAngleGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("yawAngleGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("yawAngleGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("yawAngleGain").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("yawAngleGain").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
        EditorGUI.indentLevel--;


        EditorGUI.indentLevel++;
        GUILayout.Space(10f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Altitude Hold Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("altitudeGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("altitudeGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("altitudeGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("altitudeGain").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("altitudeGain").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
        EditorGUI.indentLevel--;

        EditorGUI.indentLevel++;
        GUILayout.Space(10f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Climb Controller Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("climbGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("climbGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("climbGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("climbGain").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("climbGain").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
        EditorGUI.indentLevel--;

        EditorGUI.indentLevel++;
        GUILayout.Space(10f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Turn Controller Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("turnGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("turnGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("turnGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("turnGain").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("turnGain").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
        EditorGUI.indentLevel--;



        serializedObject.ApplyModifiedProperties();
    }
}
#endif

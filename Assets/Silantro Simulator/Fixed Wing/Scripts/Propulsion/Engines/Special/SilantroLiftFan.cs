using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Silantro;
using Oyedoyin;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroLiftFan : MonoBehaviour
{

    // ----------------------------------------- Variables
    public float fanPower;
    public float fanShaftPower;
    public float fanThrust;
    public float fanDiameter = 1f;
    [Range(0,100)]public float fanEfficiency = 86f;
    [Range(0, 80)]public float extractionRatio = 60f;
    public bool initialized;


    // ----------------------------------------- Connections
    public SilantroEngineCore core;
    public SilantroTurboFan attachedEngine;
    public SilantroController controller;
    public Transform intakePoint, exitPoint;



  
    //------------------------------------------------------------------------------------------------------------------------------------------------
    public void ReturnIgnitionCall()
    {
        StartCoroutine(ReturnIgnition());
    }
    public IEnumerator ReturnIgnition() { yield return new WaitForSeconds(0.5f); core.start = false; core.shutdown = false; }




    //------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeEngine()
    {
        

        if (controller != null && attachedEngine != null)
        {
            // --------------------------------- Run Core
            core.engine = this.transform;
            core.functionalRPM = attachedEngine.core.functionalRPM * (extractionRatio / 100f);
            core.controller = controller;
            core.intakeFan = intakePoint;
            core.liftFan = GetComponent<SilantroLiftFan>();
            core.engineType = SilantroEngineCore.EngineType.LiftFan;
            core.InitializeEngineCore();
            initialized = true;
        }
    }


    //------------------------------------------------------------------------------------------------------------------------------------------------
    void FixedUpdate()
    {
        if (initialized)
        {

            // ----------------- //Core
            core.UpdateCore();

            // ----------------- //Power
            AnalyseFan();

            core.controlInput = controller.flightComputer.processedThrottle;
        }
    }


    public float liftFactor;
    public float fanLift;
    public Vector3 liftForce;
    //------------------------------------------------------------------------------------------------------------------------------------------------
    public void AnalyseFan()
    {
        if (attachedEngine != null)
        {
            fanShaftPower = attachedEngine.Wc * (extractionRatio / 100)*1000f;
            float propellerArea = (3.142f * Mathf.Pow((3.28084f * fanDiameter), 2f)) / 4f;
            float dynamicPower = Mathf.Pow((fanShaftPower * 550f), 2 / 3f);
            float dynamicArea = core.coreFactor * Mathf.Pow((2f * controller.core.airDensity * 0.0624f * propellerArea), 1 / 3f);
            fanThrust = dynamicArea * dynamicPower;
        }
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(SilantroLiftFan))]
public class SilantroLiftfanEditor : Editor
{
    Color backgroundColor;
    Color silantroColor = new Color(1, 0.4f, 0);
    SilantroLiftFan fan;
    SerializedProperty core;

    private void OnEnable() { fan = (SilantroLiftFan)target; core = serializedObject.FindProperty("core"); }
    
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector();

        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Power Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attachedEngine"), new GUIContent("Connected Engine"));
        GUILayout.Space(3f);
        GUILayout.Space(3f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Power Extraction Settings", MessageType.None);
        GUI.color = backgroundColor;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("extractionRatio"), new GUIContent("Extraction %"));
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Shaft Power", fan.fanShaftPower.ToString("0.00 W"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fanDiameter"), new GUIContent("Fan Diameter %"));

        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Connections", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("intakePoint"), new GUIContent("Intake Fan"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("rotationAxis"), new GUIContent("Rotation Axis"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("rotationDirection"), new GUIContent("Rotation Direction"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("exitPoint"), new GUIContent("Exhaust Point"));


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Ignition Sound"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Idle Sound"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Shutdown Sound"));



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Output", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Core Power", (fan.core.corePower * fan.core.coreFactor * 100f).ToString("0.00") + " %");
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Fan Thrust", fan.fanThrust.ToString("0.0") + " N");


        serializedObject.ApplyModifiedProperties();
    }
}
#endif
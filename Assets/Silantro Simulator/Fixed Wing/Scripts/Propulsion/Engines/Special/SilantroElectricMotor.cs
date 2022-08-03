using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Silantro;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroElectricMotor : MonoBehaviour
{

    // ------------------------------------ Connections
    public SilantroBattery batteryPack;
    public SilantroController controller;
    public enum EngineState { Off, Running }
    public EngineState engineState;

    public float ratedRPM;
    public float coreRPM;
    public float coreFactor;


    public float ratedVoltage;
    public float ratedCurrent;
    public float inputCurrent;
    public float inputVoltage;
    public float voltageFactor = 1f;

    [Range(0, 100f)] public float efficiency = 50f;
    public float torque;
    public float horsePower;
    public float powerRating;
    public float controlInput;
    public float weight;
    [Range(0.01f, 2f)] public float engineAcceleration = 0.2f;



    public AudioClip motorSound;
    AudioSource boosterSound;
    public float maximumPitch = 1.5f;
    public float norminalRPM;





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void StartEngine()
    {
        if (batteryPack != null) { engineState = EngineState.Running; }
        else { Debug.Log("Engine " + this.transform.name + " cannot start..No battery attached"); }
    }
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ShutDownEngine() { engineState = EngineState.Off; }





    public void InitializeMotor()
    {
        inputCurrent = ratedCurrent * voltageFactor;
        engineState = EngineState.Off;
        if (motorSound != null) { Oyedoyin.Handler.SetupSoundSource(transform, motorSound, "Struct Sound Point", 50f, true, true, out boosterSound); }
    }



  
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Update()
    {
        norminalRPM = (ratedRPM * 0.1f) + (ratedRPM - (ratedRPM * 0.1f)) * controlInput;
        if (engineState == EngineState.Running) {coreRPM = Mathf.Lerp(coreRPM, norminalRPM, engineAcceleration * Time.fixedDeltaTime * 2); }
        else { coreRPM = Mathf.Lerp(coreRPM, 0, engineAcceleration * Time.fixedDeltaTime * 2f); }

        coreFactor = coreRPM / ratedRPM;
        boosterSound.pitch = (coreFactor * maximumPitch);
        boosterSound.volume = coreFactor;

        if (coreRPM > 1) { AnalysePower(); }
    }






    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void AnalysePower()
    {
        inputVoltage = voltageFactor * ratedVoltage;
        powerRating = inputCurrent * inputVoltage * (coreRPM / ratedRPM);
        batteryPack.outputCurrent = inputCurrent;
        batteryPack.outputVoltage = inputVoltage;

        torque = (powerRating * efficiency * 60f) / (coreRPM * 2f * 3.142f * 100f);
        horsePower = (coreRPM / ratedRPM) * (torque * coreRPM) / 5252f;

        // --------------------------- Battery Connection
        UseBattery();
    }




    // ----------------------------------------DEPLETE BATTERY------------------------------------------------------------------------------------------------------------------
    void UseBattery()
    {
        if (batteryPack)
        {
            //batteryPack.currentCapacity -= (ratedCurrent*powerInput * ratedVoltage);
            //NOT REALLY SURE HOW THIS WORKS :))
        }
    }
}


#if UNITY_EDITOR

// --------------------------------------------- TurboFan
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroElectricMotor))]
public class SilantroElectricMotorEditor : Editor
{
    Color backgroundColor;
    Color silantroColor = new Color(1.0f, 0.40f, 0f);
    SilantroElectricMotor motor;
    public int toolbarTab;
    public string currentTab;



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnEnable() { motor = (SilantroElectricMotor)target; }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector ();
        serializedObject.Update();

        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Motor Specifications", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ratedVoltage"), new GUIContent("Rated Voltage"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ratedCurrent"), new GUIContent("Rated Current"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("efficiency"), new GUIContent("Motor Efficiency"));
        GUILayout.Space(8f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ratedRPM"), new GUIContent("Rated RPM"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("engineAcceleration"), new GUIContent("Motor Acceleration"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weight"), new GUIContent("Weight"));


        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Power Settings", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("batteryPack"), new GUIContent("Battery Pack"));
        GUILayout.Space(8f);
        EditorGUILayout.LabelField("Input Voltage", motor.inputVoltage.ToString("0.0") + " Volts");
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Input Current", motor.inputCurrent.ToString("0.0") + " Amps");


        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("motorSound"), new GUIContent("Running Sound"));
        GUILayout.Space(10f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumPitch"), new GUIContent("Motor Maximum Pitch"));


        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Performance", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Motor State", motor.engineState.ToString());
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Motor Power", (motor.powerRating / 1000).ToString("0.00") + " kW");
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Shaft Horsepower", motor.horsePower.ToString("0.0") + " Hp");
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Core Speed", motor.coreRPM.ToString("0.0") + " RPM");


        serializedObject.ApplyModifiedProperties();
    }
}
#endif

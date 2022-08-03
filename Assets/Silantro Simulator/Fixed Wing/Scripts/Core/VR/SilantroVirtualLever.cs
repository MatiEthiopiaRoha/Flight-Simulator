using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroVirtualLever : MonoBehaviour
{
    // ------------------------------ Selectibles
    public enum LeverType { Throttle, ControlStick, Component, Switch }
    public LeverType leverType = LeverType.ControlStick;

    public enum RotationState { Normal, Invert }
    public RotationState rotationState = RotationState.Normal;

    public enum RotationAxis { X, Y }
    public RotationAxis rollAxis = RotationAxis.X;
    public RotationAxis leverAxis = RotationAxis.X;
    public RotationAxis pitchAxis = RotationAxis.X;
    public enum LeverAction { SelfCentering, NonCentering }
    public LeverAction leverAction = LeverAction.NonCentering;

    public enum AxisState { Normal, Inverted }
    public AxisState pitchAxisState = AxisState.Normal;
    public AxisState rollAxisState = AxisState.Normal;
    public AxisState leverAxisState = AxisState.Normal;


    // ------------------------------ Connections
    public Transform leverCore;
    public Transform vehicle;
    public Transform handPoint;


    [System.Serializable] public class InteractionEvent : UnityEvent { }
    public InteractionEvent raiseFunction = new InteractionEvent();
    public InteractionEvent lowerFunction = new InteractionEvent();
    public float raiseCallPoint = 0.8f;
    public float lowerCallPoint = 0.2f;
    public enum SwitchPoint { Raised, Lowered }
    public SwitchPoint switchPoint = SwitchPoint.Lowered;

    public Material bulbMaterial;
    public float maximumBulbEmission = 10f;
    private Color baseColor, finalColor;


    // ------------------------------ Output
    public float leverOutput;
    public float pitchOutput, rollOutput;

    // ------------------------------ Value Storage
    public Vector2 angle, value;
    public Vector2 deflectionLimit = new Vector2(30, 30);
    public Vector3 handPosition;

    // ------------------------------ Triggers
    public bool rightControlHold;
    public bool leftControlHold;
    public bool leverHeld;

    // ------------------------------ Control Variables
    private float leftTriggerInput;
    private float rightTriggerInput;
    private float leftPalmInput;
    private float rightPalmInput;
    //float xRotation, yRotation, zRotation;

    public float snapSpeed = 10f;
    public float maximumDeflection = 20f;
    public float maximumPitchDeflection = 20f, maximumRollDeflection = 20f;


    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeLever()
    {
        if (leverType == LeverType.ControlStick) { deflectionLimit = new Vector2(maximumRollDeflection, maximumPitchDeflection); }

        if (leverType == LeverType.Component)
        {
            if (leverAxis == RotationAxis.X) { deflectionLimit = new Vector2(maximumDeflection / 2, 0); }
            if (leverAxis == RotationAxis.Y) { deflectionLimit = new Vector2(0, maximumDeflection / 2); }
        }

        if (leverType == LeverType.Throttle)
        {
            if (leverAxis == RotationAxis.X) { deflectionLimit = new Vector2(maximumDeflection / 2, 0); }
            if (leverAxis == RotationAxis.Y) { deflectionLimit = new Vector2(0, maximumDeflection / 2); }
        }

        if (leverType == LeverType.Switch)
        {
            if (leverAxis == RotationAxis.X) { deflectionLimit = new Vector2(maximumDeflection / 2, 0); }
            if (leverAxis == RotationAxis.Y) { deflectionLimit = new Vector2(0, maximumDeflection / 2); }
        }
    }




    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            //Component e.g Gear Lever
            if (leverType == LeverType.Component && leftControlHold) { leverHeld = true; }
            //Control Stick
            if (leverType == LeverType.ControlStick && rightControlHold) { leverHeld = true; }
            //Throttle
            if (leverType == LeverType.Throttle && leftControlHold) { leverHeld = true; }

            //HAND DATA
            handPosition = other.transform.position;
        }
    }


    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand")) { leverHeld = false; }
    }



    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void Update()
    {
        if (handPoint != null)
        {
            //ACCESS TOUCH INPUTS
            CollectTouchState();


            //PROCESS ROTATION MAIN
            if (leverHeld) { GrabLever(); }

            if (leverAction == LeverAction.SelfCentering && !leverHeld)
            {
                //Reset Core
                value = Vector2.MoveTowards(value, Vector2.zero, Time.deltaTime * snapSpeed);
                //if (value == Vector2.zero) { enabled = false; }
                leverCore.localRotation = Quaternion.LookRotation(Vector3.SlerpUnclamped(Vector3.SlerpUnclamped(new Vector3(-1, -1, 1),
                    new Vector3(-1, 1, 1), value.x * deflectionLimit.x / 90 + .5f),
                    Vector3.SlerpUnclamped(new Vector3(1, -1, 1),
                    new Vector3(1, 1, 1), value.x * deflectionLimit.x / 90 + .5f),
                    value.y * deflectionLimit.y / 90 + .5f), Vector3.up);
            }

            RefreshInput();
            if (leverType == LeverType.Switch) { RefreshLever(); }


            //RETURN TO BASE ROTATION
            if (leverType == LeverType.Component || leverType == LeverType.Throttle) { if (!leftControlHold && leverHeld) { leverHeld = false; } }
            if (leverType == LeverType.ControlStick) { if (!rightControlHold && leverHeld) { leverHeld = false; } }
        }
    }



    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    void RefreshLever()
    {
        if (leverType == LeverType.Switch)
        {
            if (leverAxis == RotationAxis.X) { leverOutput = 1 - ((-value.x + 1) / 2); }
            if (leverAxis == RotationAxis.Y) { leverOutput = 1 - ((-value.y + 1) / 2); }
        }

        if (leverOutput < lowerCallPoint && switchPoint != SwitchPoint.Lowered) { switchPoint = SwitchPoint.Lowered; lowerFunction.Invoke(); bulbMaterial.color = Color.green; baseColor = Color.green; }
        if (leverOutput > raiseCallPoint && switchPoint != SwitchPoint.Raised) { switchPoint = SwitchPoint.Raised; raiseFunction.Invoke(); bulbMaterial.color = Color.red; baseColor = Color.red; }

        // ------------------- Set
        finalColor = baseColor * Mathf.LinearToGammaSpace(maximumBulbEmission);
        if (bulbMaterial != null) { bulbMaterial.SetColor("_EmissionColor", finalColor); }
    }



    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public Vector2 RefreshInput()
    {
        Vector2 output = Vector2.zero;

        // ------------------------------------------------ Collect Output
        if (leverType == LeverType.ControlStick)
        {
            if (pitchAxisState == AxisState.Normal) { pitchOutput = -value.y; } else { pitchOutput = value.y; }
            if (rollAxisState == AxisState.Normal) { rollOutput = -value.x; } else { rollOutput = value.x; }

            output = new Vector2(rollOutput, pitchOutput);
        }

        if (leverType == LeverType.Component || leverType == LeverType.Throttle)
        {
            if (leverAxis == RotationAxis.X) { leverOutput = leverAxisState == AxisState.Inverted ? 1 - ((-value.x + 1) / 2) : (-value.x + 1) / 2; output = new Vector2(leverOutput, 0); }
            if (leverAxis == RotationAxis.Y) { leverOutput = leverAxisState == AxisState.Inverted ? 1 - ((-value.y + 1) / 2) : (-value.y + 1) / 2; output = new Vector2(leverOutput, 0); }
        }

        return output;
    }




    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    void GrabLever()
    {
        handPoint.position = handPosition;
        handPoint.localPosition = new Vector3(handPoint.localPosition.x, handPoint.localPosition.y, Mathf.Abs(handPoint.localPosition.z));
        angle.x = Vector2.SignedAngle(new Vector2(handPoint.localPosition.y, handPoint.localPosition.z), Vector2.up);
        angle.y = Vector2.SignedAngle(new Vector2(handPoint.localPosition.x, handPoint.localPosition.z), Vector2.up);

        angle = new Vector2(Mathf.Clamp(angle.x, -deflectionLimit.x, deflectionLimit.x), Mathf.Clamp(angle.y, -deflectionLimit.y, deflectionLimit.y));
        value = new Vector2(angle.x / (deflectionLimit.x + Mathf.Epsilon), angle.y / (deflectionLimit.y + Mathf.Epsilon));

        leverCore.localRotation = Quaternion.LookRotation(Vector3.SlerpUnclamped(Vector3.SlerpUnclamped(new Vector3(-1, -1, 1), new Vector3(-1, 1, 1),
            value.x * deflectionLimit.x / 90 + .5f), Vector3.SlerpUnclamped(new Vector3(1, -1, 1), new Vector3(1, 1, 1),
            value.x * deflectionLimit.x / 90 + .5f), value.y * deflectionLimit.y / 90 + .5f), Vector3.up);
    }



    //Remove the line comments to use VR
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    void CollectTouchState()
    {
        //RIGHT CONTROLLER
        //rightTriggerInput = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        //rightPalmInput = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch);

        //LEFT CONTROLLER
        //leftTriggerInput = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        //leftPalmInput = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch);

        //STATE
        if (rightTriggerInput > 0.9f && rightPalmInput > 0.9f) { rightControlHold = true; } else { rightControlHold = false; }
        if (leftTriggerInput > 0.9f && leftPalmInput > 0.9f) { leftControlHold = true; } else { leftControlHold = false; }
    }
}


#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroVirtualLever))]
public class SilantroVirtualLeverEditor : Editor
{
    Color backgroundColor;
    Color silantroColor = new Color(1, 0.4f, 0);
    SilantroVirtualLever lever;
    private void OnEnable() { lever = (SilantroVirtualLever)target; }


    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector ();
        serializedObject.UpdateIfRequiredOrScript();

        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Lever Config", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("leverType"), new GUIContent("Function"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("leverAction"), new GUIContent("Mechanism"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("leverCore"), new GUIContent("Core"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("handPoint"), new GUIContent("Grab Position"));
        if (lever.leverAction == SilantroVirtualLever.LeverAction.SelfCentering)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("snapSpeed"), new GUIContent("Centering Speed"));
        }



        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Deflection Settings", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        if (lever.leverType == SilantroVirtualLever.LeverType.ControlStick)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchAxis"), new GUIContent("Pitch Axis"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchAxisState"), new GUIContent("Pitch Axis State"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rollAxis"), new GUIContent("Roll Axis"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rollAxisState"), new GUIContent("Roll Axis State"));
        }
        else if (lever.leverType == SilantroVirtualLever.LeverType.Component)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("leverAxis"), new GUIContent("Lever Axis"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("leverAxisState"), new GUIContent("Axis State"));
        }
        else if (lever.leverType == SilantroVirtualLever.LeverType.Throttle)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("leverAxis"), new GUIContent("Throttle Axis"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("leverAxisState"), new GUIContent("Throttle State"));

        }
        else if (lever.leverType == SilantroVirtualLever.LeverType.Switch)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("leverAxis"), new GUIContent("Lever Axis"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("leverAxisState"), new GUIContent("Axis State"));
        }



        if (lever.leverType == SilantroVirtualLever.LeverType.ControlStick)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumPitchDeflection"), new GUIContent("Full Pitch Deflection"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRollDeflection"), new GUIContent("Full Roll Deflection"));
        }
        else if (lever.leverType == SilantroVirtualLever.LeverType.Component)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumDeflection"), new GUIContent("Full Lever Deflection"));
        }
        else if (lever.leverType == SilantroVirtualLever.LeverType.Switch)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumDeflection"), new GUIContent("Full Lever Deflection"));
        }
        else if (lever.leverType == SilantroVirtualLever.LeverType.Throttle)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumDeflection"), new GUIContent("Full Throttle Deflection"));
        }


        if (lever.leverType != SilantroVirtualLever.LeverType.Switch)
        {
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Output", MessageType.None);
            GUI.color = backgroundColor;
            if (lever.leverType == SilantroVirtualLever.LeverType.ControlStick)
            {
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Roll Ouput", lever.rollOutput.ToString("0.000"));
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Pitch Ouput", lever.pitchOutput.ToString("0.000"));
            }
            else if (lever.leverType == SilantroVirtualLever.LeverType.Component)
            {
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Lever Ouput", lever.leverOutput.ToString("0.000"));
            }
            else if (lever.leverType == SilantroVirtualLever.LeverType.Throttle)
            {
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Throttle Ouput", lever.leverOutput.ToString("0.000"));
            }
        }

        if (lever.leverType == SilantroVirtualLever.LeverType.Switch)
        {

            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Switch Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lowerCallPoint"), new GUIContent("Lower Call Point"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("raiseCallPoint"), new GUIContent("Raise Call Point"));

            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lowerFunction"), new GUIContent("Lower Function"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("raiseFunction"), new GUIContent("Raise Function"));
        }


        serializedObject.ApplyModifiedProperties();
    }
}
#endif

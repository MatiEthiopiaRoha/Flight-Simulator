using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif



public class SilantroComputer : MonoBehaviour
{
    public enum ComputerType { DataProcessing, Guidance }
    public ComputerType computerType = ComputerType.DataProcessing;
    public enum WeaponType { Bomb, Missile, Rocket }
    public WeaponType weaponType = WeaponType.Missile;
    public enum HomingType { ActiveRadar, SemiActiveRadar }
    public HomingType homingType = HomingType.SemiActiveRadar;


    // ------------------------------------------------ Connections
    public SilantroMunition munition;
    public SilantroRadar supportRadar;
    GameObject radarCore;
    public Transform Target;


    // ------------------------------------------------ Variables
    public bool seeking;
    public float range = 10000f;
    public float pingRate = 5;
    public float pingTime;
    public float actualPingRate;
    public bool displayBounds;

    public float currentSpeed;
    public float machSpeed, soundSpeed;
    public float currentAltitude;
    public float airDensity;
    public float ambientTemperature;
    public float ambientPressure; float altimeter; float a; float b;

    public Collider[] visibleObjects;
    public List<GameObject> processedTargets;
    public string targetID = "Unassigned";
    const string headerContainer = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string qualifierContainer = "1234567890";
    string header;
    string qualifier;
    public string WeaponID = "Unassigned";

    // ------------------------------------------------ Target Data
    Vector3 TargetMirror;
    Vector3 TargetPosition;
    Vector3 TargetPositionChange;
    Vector3 desiredRotation;
    public float navigationalConstant = 5;
    public float maxRotation = 50f;
    public bool active;

    public float viewDirection;
    public float minimumLockDirection = 0.5f;
    public float distanceToTarget;



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeComputer()
    {
        if (computerType == ComputerType.Guidance)
        {
            WeaponID = GenerateTrackID();
            actualPingRate = (1f / pingRate);
            pingTime = 0.0f;

            //SETUP CORE
            radarCore = new GameObject(munition.transform.name + " Radar Core");
            radarCore.transform.parent = this.transform;
            radarCore.transform.localPosition = new Vector3(0, 0, 0);
            //SETUP MISSILE
            if (weaponType == WeaponType.Missile)
            {
                SilantroTransponder ponder = GetComponent<SilantroTransponder>();
                if (ponder)
                {
                    ponder.TrackingID = WeaponID;
                }
            }
        }
    }



    // -------------------------------------------------------------//GENERATE TRACKING ID---------------------------------------------------------------------------------------------
    public string GenerateTrackID()
    {
        header = string.Empty; qualifier = string.Empty;
        for (int i = 0; i < 3; i++) { header += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[UnityEngine.Random.Range(0, "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Length)]; }
        for (int j = 0; j < 2; j++) { qualifier += "1234567890"[UnityEngine.Random.Range(0, "1234567890".Length)]; }
        return header + qualifier;
    }


    // -----------------------------------------------------------//DISPLAY CORE SEEKER-----------------------------------------------------------------------------------------------
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (displayBounds && computerType == ComputerType.Guidance && seeking)
        {
            if (radarCore && munition)
            {
                // Search View
                float radius = range;
                Vector3 center = radarCore.transform.position;
                Vector3 normal = munition.transform.up;
                Vector3 from = Quaternion.AngleAxis(120f, normal) * radarCore.transform.forward * -1;
                Handles.color = new Color(Color.yellow.r, Color.yellow.b, Color.yellow.b, 0.1f);
                Handles.DrawSolidArc(center, normal, from, 120, radius);
            }
            else
            {
                // Search View
                float radius = range;
                Vector3 center = this.transform.position;
                if (munition)
                {
                    Vector3 normal = munition.transform.up;
                    Vector3 from = Quaternion.AngleAxis(120f, normal) * this.transform.forward * -1;
                    Handles.color = new Color(Color.yellow.r, Color.yellow.b, Color.yellow.b, 0.1f);
                    Handles.DrawSolidArc(center, normal, from, 120, radius);
                }
            }
            //DRAW LINE OF SIGHT
            if (Target)
            {
                Handles.color = Color.red;
                Handles.DrawLine(transform.position, Target.position);
            }
        }
    }
#endif




    Collider[] filterPool;
    // ----------------------------------------------------------------------//SEARCH SWEEP------------------------------------------------------------------------------------
    void Ping()
    {
        pingTime = 0.0f;
        //SEARCH
        visibleObjects = Physics.OverlapSphere(transform.position, range);
        processedTargets.Clear();

        if (filterPool != visibleObjects)
        {

            //FILTER OUT OBJECTS
            foreach (Collider filterCollider in visibleObjects)
            {
                //AVOID SELF DETECTION
                if (!filterCollider.transform.IsChildOf(munition.transform))
                {
                    //SEPARATE OBJECTS
                    SilantroTransponder transponder;
                    //CHECK PARENT BODY
                    transponder = filterCollider.gameObject.GetComponent<SilantroTransponder>();
                    if (transponder == null)
                    {
                        //CHECK IN CHILDREN
                        transponder = filterCollider.gameObject.GetComponentInChildren<SilantroTransponder>();
                    }
                    if (transponder != null)
                    {
                        //REGISTER DETECTION
                        processedTargets.Add(filterCollider.gameObject);
                        //SET VARIABLES
                        transponder.isTracked = true;
                        transponder.TrackingID = WeaponID;

                        if (transponder.AssignedID == "Default") { transponder.AssignedID = GenerateTrackID(); }
                    }
                }
            }

            filterPool = visibleObjects;
        }
        //RESET
        Target = null;

        //SWEEP MODES
        if (homingType == HomingType.ActiveRadar)
        {
            ActiveTargeting();
        }
        if (homingType == HomingType.SemiActiveRadar)
        {
            SemiActiveTargeting();
            if(supportRadar != null)
            {
                if(Target == null && supportRadar.lockedTarget != null) { Target = supportRadar.lockedTarget.body.transform; seeking = true; }
            }
        }
        munition.target = Target;
    }




    // -----------------------------------------------------------------------------------//ACTIVE HOMING-----------------------------------------------------------------------
    void ActiveTargeting()
    {
        foreach (GameObject track in processedTargets)
        {
            if (track != null)
            {
                //FIND ATTACHED TRANSPONDER
                if (track.GetComponent<SilantroTransponder>())
                {
                    SilantroTransponder ponder = track.GetComponent<SilantroTransponder>();
                    if (ponder != null)
                    {
                        if (ponder.AssignedID == targetID)
                        {
                            Target = track.transform;//Assign visible target
                        }
                    }
                }

                //FIND ATTACHED TRANSPONDER in CHILDREN
                if (track.GetComponentInChildren<SilantroTransponder>())
                {
                    SilantroTransponder ponder = track.GetComponentInChildren<SilantroTransponder>();
                    if (ponder != null)
                    {
                        if (ponder.AssignedID == targetID)
                        {
                            Target = track.transform;//Assign visible target
                        }
                    }
                }
            }
        }

        homingType = HomingType.ActiveRadar;
    }


    // ---------------------------------------------------------------------SEMI ACTIVE HOMING-------------------------------------------------------------------------------------
    void SemiActiveTargeting()
    {
        //COLLECT TARGETS FROM PARENT RADAR
        if (supportRadar != null) { processedTargets = supportRadar.processedObjects; }
        //AUGUMENT WITH SUPPORT RADAR
        if (distanceToTarget > range)
        {
            if (supportRadar != null)
            {
                foreach (GameObject track in processedTargets)
                {
                    //FIND ATTACHED TRANSPONDER
                    if (track != null)
                    {
                        if (track.GetComponent<SilantroTransponder>())
                        {
                            SilantroTransponder ponder = track.GetComponent<SilantroTransponder>();
                            if (ponder != null)
                            {
                                if (ponder.AssignedID == targetID)
                                {
                                    Target = track.transform;//Assign visible target
                                }
                            }
                        }
                        //
                        //FIND ATTACHED TRANSPONDER in CHILDREN
                        if (track.GetComponentInChildren<SilantroTransponder>())
                        {
                            SilantroTransponder ponder = track.GetComponentInChildren<SilantroTransponder>();
                            if (ponder != null)
                            {
                                if (ponder.AssignedID == targetID)
                                {
                                    Target = track.transform;//Assign visible target
                                }
                            }
                        }
                    }
                }
            }

            homingType = HomingType.SemiActiveRadar;
        }
        else
        {
            //USE RADAR ONCE TAREGT IS WITHIN RANGE COVER
            ActiveTargeting();
        }
    }


    public static Vector3 Calculate(Vector3 ownPos, Vector3 ownVel, Vector3 targetPos, Vector3 targetVel, float n)
    {
        Vector3 vector = targetPos - ownPos;
        Vector3 vector2 = targetVel - ownVel;
        Vector3 rhs = Vector3.Cross(vector, vector2) / Vector3.Dot(vector, vector);
        return Vector3.Cross(n * vector2, rhs);
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void Rotate(Quaternion desiredRotation)
    {
         munition.transform.rotation = Quaternion.RotateTowards(munition.transform.rotation, desiredRotation, Time.deltaTime * maxRotation);
    }


    void Update()
    {
        if (active && computerType == ComputerType.Guidance)
        {
            //TRACK TARGET
            if (Target != null) { distanceToTarget = Vector3.Distance(this.transform.position, Target.position); }

            //SEND OUT PING
            pingTime += Time.deltaTime;
            if (pingTime >= actualPingRate) { Ping(); }

            //SPIN CORE
            if (seeking) { radarCore.transform.Rotate(new Vector3(0, pingRate * 100 * Time.deltaTime, 0)); }

            //SEND CALCULATION DATA
            if (munition != null) { CalculateData(); }

            //SEEK
            if (computerType == ComputerType.Guidance)
            {
                if (seeking && Target != null)
                {
                    SilantroNavigation();
                    //ProNavigation();
                }
            }
        }
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void FixedUpdate()
    {
       
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void CalculateData()
    {
        //CALCULATE BASE DATA
        currentAltitude = munition.gameObject.transform.position.y * 3.28084f;//Convert altitude from meter to feet
        currentSpeed = munition.munition.velocity.magnitude * 1.944f;//Speed in knots
                                                                     //CALCULATE DENSITY
        float kelvinTemperatrue;
        kelvinTemperatrue = ambientTemperature + 273.15f;
        airDensity = (ambientPressure * 1000f) / (287.05f * kelvinTemperatrue);
        //CALCULATE AMBIENT DATA
        float a1; float a2;
        a1 = 0.000000003f * currentAltitude * currentAltitude;
        a2 = 0.0021f * currentAltitude; ambientTemperature = a1 - a2 + 15.443f;
        //CALCULATE PRESSURE
        a = 0.0000004f * currentAltitude * currentAltitude;
        b = (0.0351f * currentAltitude);
        ambientPressure = (a - b + 1009.6f) / 10f;
        //CALCULATE MACH SPEED
        soundSpeed = Mathf.Pow((1.2f * 287f * (273.15f + ambientTemperature)), 0.5f);
        machSpeed = (currentSpeed / 1.944f) / soundSpeed;
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void SilantroNavigation()
    {
        //1. STOP SEEKING IF TARGET IS OUT OF VIEW
        Vector3 targetDirection = (Target.transform.position - munition.transform.position).normalized;
        viewDirection = Vector3.Dot(targetDirection, transform.forward);
        //if (viewDirection < minimumLockDirection) { seeking = false; Target = null; Debug.Log("Missile " + munition.transform.name + " " + munition.computer.WeaponID + " lost track of target. Reason: Target out of navigation range"); }

        //2. STOP SEEKING IF TARGET IS BEHIND THE MISSILE
        if (Vector3.Dot(targetDirection, transform.forward) < 0) { seeking = false; Target = null; Debug.Log("Missile " + munition.transform.name + " " + munition.computer.WeaponID + " lost track of target. Reason: Target overshoot"); }


        if (Target != null)
        {
            //DETERMINE TARGET POSITION VARIATION
            TargetMirror = TargetPosition;
            TargetPosition = Target.position - transform.position;
            TargetPositionChange = TargetPosition - TargetMirror;
            TargetPositionChange = TargetPositionChange - Vector3.Project(TargetPositionChange, TargetPosition);
            desiredRotation = (Time.deltaTime * TargetPosition) + (TargetPositionChange * navigationalConstant);

            //FACE TARGET
            if (seeking && active)
            {
                Rotate(Quaternion.LookRotation(desiredRotation, transform.up));
            }
        }
    }
}






#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroComputer))]
public class SilantroGuidanceEditor : Editor
{
    Color backgroundColor;
    Color silantroColor = new Color(1, 0.4f, 0);

    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector(); 
        SilantroComputer computer = (SilantroComputer)target;

        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Computer Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("computerType"), new GUIContent(" "));



        //1. DATA PROCESSING COMPUTER ONLY
        if (computer.computerType == SilantroComputer.ComputerType.DataProcessing)
        {
            GUILayout.Space(2f);
            GUI.color = Color.yellow;
            EditorGUILayout.HelpBox("No longer required, please remove", MessageType.Warning);
            GUI.color = backgroundColor;
        }

        //2. GUIDANCE COMPUTER
        if (computer.computerType == SilantroComputer.ComputerType.Guidance)
        {
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Performance", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Mach Speed", computer.machSpeed.ToString("0.00"));
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Altitude", computer.currentAltitude.ToString("0.0") + " ft");
            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Sensor Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("range"), new GUIContent("Range"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pingRate"), new GUIContent("Ping Rate"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("displayBounds"), new GUIContent("Display Bounds"));
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Weapon ID", computer.WeaponID);

            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Target Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("homingType"), new GUIContent("Homing Type"));
            GUILayout.Space(3f);
            if(computer.homingType == SilantroComputer.HomingType.SemiActiveRadar && computer.supportRadar != null) { EditorGUILayout.LabelField("Parent Radar", computer.supportRadar.connectedAircraft.name); }
            if (computer.Target)
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Target", computer.Target.name);
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Target Distance", computer.distanceToTarget.ToString("0.0") + " m");
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Target Direction", computer.viewDirection.ToString());
            }
            else
            {
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Target", "Unassigned");
            }
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Target ID", computer.targetID);
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Seeking", computer.seeking.ToString());

            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Navigation Control", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxRotation"), new GUIContent("Rotation Rate"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("navigationalConstant"), new GUIContent("Navigation Constant"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumLockDirection"), new GUIContent("Minimum Lock Direction"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class SilantroComputerConfig : MonoBehaviour { public SilantroFlightComputer fcs; public float pitchOutput; private void Update() { pitchOutput = fcs.pitchRateSolver.output; }}




#if UNITY_EDITOR
[CustomEditor(typeof(SilantroComputerConfig))]
public class SilantroComputerConfigEditor : Editor
{
    Color backgroundColor;
    Color silantroColor = new Color(1, 0.4f, 0);
    SilantroComputerConfig board;
    public int toolbarTab;
    public string currentTab;
    SerializedObject computer;

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnEnable() { board = (SilantroComputerConfig)target; computer = new SerializedObject(serializedObject.FindProperty("fcs").objectReferenceValue); }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector();
        serializedObject.Update();
        computer.Update();
        

        GUILayout.Space(3f);
        GUI.color = silantroColor;
        if (GUILayout.Button("Return to Computer")){ Selection.activeGameObject = board.fcs.gameObject;}
        GUI.color = backgroundColor;

        GUILayout.Space(10f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Gain Sheet Config", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        toolbarTab = GUILayout.Toolbar(toolbarTab, new string[] { "Save Gains", "Load Gains" });
        GUILayout.Space(5f);
        switch (toolbarTab)
        {
            case 0: currentTab = "Save Gains"; break;
            case 1: currentTab = "Load Gains"; break;
        }
        switch (currentTab)
        {
            case "Save Gains":
                EditorGUILayout.PropertyField(computer.FindProperty("aircraftIdentifier"), new GUIContent("Sheet Label"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(computer.FindProperty("saveLocation"), new GUIContent("Save Location"));
                GUILayout.Space(5f);
                if (board.fcs.aircraftIdentifier != null && board.fcs.aircraftIdentifier.Length > 5)
                {
                    GUI.color = silantroColor;
                    if (GUILayout.Button("Save")) { board.fcs.SaveGain(); }
                    GUI.color = backgroundColor;
                }
                break;

            case "Load Gains":
                EditorGUILayout.PropertyField(computer.FindProperty("gainSetInput"), new GUIContent("Gain Sheet"));
                GUILayout.Space(5f);
                if (board.fcs.gainSetInput != null)
                {
                    GUI.color = silantroColor;
                    if (GUILayout.Button("Load")) { board.fcs.LoadGain(); }
                    GUI.color = backgroundColor;
                }
                break;
        }

        GUILayout.Space(20f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Gain Tuning Guide", MessageType.Info);
        GUI.color = backgroundColor;
        GUILayout.Space(5f);
        Texture background = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Silantro Simulator/Fixed Wing/Scripts/Editor/Input/Tuning Table.png", typeof(Texture));
        GUILayout.Label(background);
        GUILayout.Space(20f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Gain System", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        if (board.fcs.operationMode != SilantroFlightComputer.AugmentationType.Manual)
        {
            EditorGUILayout.PropertyField(computer.FindProperty("gainSystem"));
        }


        

        GUILayout.Space(5f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Throttle Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("throttleSolver").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("throttleSolver").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("throttleSolver").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUI.enabled = false;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("throttleSolver").FindPropertyRelative("output"), new GUIContent("Ouput"));
        GUI.enabled = true;

        EditorGUILayout.PropertyField(computer.FindProperty("throttleSolver").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("throttleSolver").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
        

        GUILayout.Space(20f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Inner-Loop controllers. The output variables from these controllers drive the aircraft control surfaces", MessageType.Info);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);

        if (board.fcs.gainSystem == SilantroFlightComputer.GainSystem.Static)
        {
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Roll Rate Gain", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("rollRateSolver").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("rollRateSolver").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("rollRateSolver").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
            GUI.enabled = false;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("rollRateSolver").FindPropertyRelative("output"), new GUIContent("Ouput"));
            GUI.enabled = true;

            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(computer.FindProperty("rollRateSolver").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("rollRateSolver").FindPropertyRelative("maximum"), new GUIContent("Maximum"));


            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Pitch Rate Gain", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("pitchRateSolver").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("pitchRateSolver").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("pitchRateSolver").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
            GUI.enabled = false;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchOutput"), new GUIContent("Ouput"));
            GUI.enabled = true;

            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(computer.FindProperty("pitchRateSolver").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("pitchRateSolver").FindPropertyRelative("maximum"), new GUIContent("Maximum"));

            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Yaw Rate Gain", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("yawRateSolver").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("yawRateSolver").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("yawRateSolver").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
            GUI.enabled = false;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("yawRateSolver").FindPropertyRelative("output"), new GUIContent("Ouput"));
            GUI.enabled = true;

            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(computer.FindProperty("yawRateSolver").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("yawRateSolver").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
        }
        else
        {
            


            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Roll Rate Gain Schedule", MessageType.None);
            GUI.color = backgroundColor;

            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Gain Schedule Config. Determines how the roll rate gains will be interpolated over various speed points to" +
                " compensate for the force variation over the flight envelope", MessageType.Info);
            GUI.color = backgroundColor;
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(computer.FindProperty("baseRollGain").FindPropertyRelative("factorType"), new GUIContent("Control Factor"));
            GUILayout.Space(2f);

            if (board.fcs.baseRollGain.factorType == Oyedoyin.Gain.FactorType.Mach)
            {
                board.fcs.cruiseRollGain.factorType = Oyedoyin.Gain.FactorType.Mach;
                board.fcs.performanceRollGain.factorType = Oyedoyin.Gain.FactorType.Mach;
                EditorGUILayout.PropertyField(computer.FindProperty("cruiseRollGain").FindPropertyRelative("factor"), new GUIContent("Cruise Mach Point","Mach speed to start using cruise gains"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(computer.FindProperty("performanceRollGain").FindPropertyRelative("factor"), new GUIContent("Perf Mach Point", "Mach speed to start using performance gains"));
            }
            else
            {
                board.fcs.cruiseRollGain.factorType = Oyedoyin.Gain.FactorType.KnotSpeed;
                board.fcs.performanceRollGain.factorType = Oyedoyin.Gain.FactorType.KnotSpeed;
                EditorGUILayout.PropertyField(computer.FindProperty("cruiseRollGain").FindPropertyRelative("factor"), new GUIContent("Cruise Knot Point", "Speed (in knots) to start using cruise gains"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(computer.FindProperty("performanceRollGain").FindPropertyRelative("factor"), new GUIContent("Perf Knot Point", "Speed (in knots) to start using cruise gains"));
            }
            GUILayout.Space(8f);
            GUI.color = Color.grey;
            EditorGUILayout.HelpBox("Base Roll Gain Set", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("baseRollGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("baseRollGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("baseRollGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
           

            GUILayout.Space(2f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Cruise Roll Gain Set", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("cruiseRollGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("cruiseRollGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("cruiseRollGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));


            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Performance Roll Gain Set", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("performanceRollGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("performanceRollGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("performanceRollGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));

            GUILayout.Space(5f);
            GUI.color = Color.grey;
            EditorGUILayout.HelpBox("Current Roll Gain Set", MessageType.None);
            GUI.color = backgroundColor;
            GUI.enabled = false;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("rollRateSolver").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("rollRateSolver").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("rollRateSolver").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("rollRateSolver").FindPropertyRelative("output"), new GUIContent("Ouput"));
            GUI.enabled = true;

            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("rollRateSolver").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("rollRateSolver").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
           



            GUILayout.Space(20f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Pitch Rate Gain Schedule", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Gain Schedule Config. Determines how the pitch rate gains will be interpolated over various speed points to" +
                " compensate for the force variation over the flight envelope", MessageType.Info);
            GUI.color = backgroundColor;
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(computer.FindProperty("basePitchGain").FindPropertyRelative("factorType"), new GUIContent("Control Factor"));
            GUILayout.Space(2f);

            if (board.fcs.basePitchGain.factorType == Oyedoyin.Gain.FactorType.Mach)
            {
                board.fcs.cruisePitchGain.factorType = Oyedoyin.Gain.FactorType.Mach;
                board.fcs.performancePitchGain.factorType = Oyedoyin.Gain.FactorType.Mach;
                EditorGUILayout.PropertyField(computer.FindProperty("cruisePitchGain").FindPropertyRelative("factor"), new GUIContent("Cruise Mach Point"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(computer.FindProperty("performancePitchGain").FindPropertyRelative("factor"), new GUIContent("Perf Mach Point"));
            }
            else
            {
                board.fcs.cruisePitchGain.factorType = Oyedoyin.Gain.FactorType.KnotSpeed;
                board.fcs.performancePitchGain.factorType = Oyedoyin.Gain.FactorType.KnotSpeed;
                EditorGUILayout.PropertyField(computer.FindProperty("cruisePitchGain").FindPropertyRelative("factor"), new GUIContent("Cruise Knot Point"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(computer.FindProperty("performancePitchGain").FindPropertyRelative("factor"), new GUIContent("Perf Knot Point"));
            }
            GUILayout.Space(8f);
            GUI.color = Color.grey;
            EditorGUILayout.HelpBox("Base Pitch Gain Set", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
          
            EditorGUILayout.PropertyField(computer.FindProperty("basePitchGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("basePitchGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("basePitchGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));


            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Cruise Pitch Gain Set", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("cruisePitchGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("cruisePitchGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("cruisePitchGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));


            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Performance Pitch Gain Set", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("performancePitchGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("performancePitchGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("performancePitchGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));

            GUILayout.Space(5f);
            GUI.color = Color.grey;
            EditorGUILayout.HelpBox("Current Pitch Gain Set", MessageType.None);
            GUI.color = backgroundColor;
            GUI.enabled = false;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("pitchRateSolver").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("pitchRateSolver").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("pitchRateSolver").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("pitchRateSolver").FindPropertyRelative("output"), new GUIContent("Ouput"));
            GUI.enabled = true;

            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("pitchRateSolver").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("pitchRateSolver").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
            //EditorGUI.indentLevel--;





            GUILayout.Space(20f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Yaw Rate Gain Schedule", MessageType.None);
            GUI.color = backgroundColor;

            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Gain Schedule Config. Determines how the yaw rate gains will be interpolated over various speed points to" +
                " compensate for the force variation over the flight envelope", MessageType.Info);
            GUI.color = backgroundColor;
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(computer.FindProperty("baseYawGain").FindPropertyRelative("factorType"), new GUIContent("Control Factor", "Gains will be interpolated based on the aircraft mach speed"));
            GUILayout.Space(2f);

            if (board.fcs.baseYawGain.factorType == Oyedoyin.Gain.FactorType.Mach)
            {
                board.fcs.cruiseYawGain.factorType = Oyedoyin.Gain.FactorType.Mach;
                board.fcs.performanceYawGain.factorType = Oyedoyin.Gain.FactorType.Mach;
                EditorGUILayout.PropertyField(computer.FindProperty("cruiseYawGain").FindPropertyRelative("factor"), new GUIContent("Cruise Mach Point"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(computer.FindProperty("performanceYawGain").FindPropertyRelative("factor"), new GUIContent("Perf Mach Point"));
            }
            else
            {
                board.fcs.cruiseYawGain.factorType = Oyedoyin.Gain.FactorType.KnotSpeed;
                board.fcs.performanceYawGain.factorType = Oyedoyin.Gain.FactorType.KnotSpeed;
                EditorGUILayout.PropertyField(computer.FindProperty("cruiseYawGain").FindPropertyRelative("factor"), new GUIContent("Cruise Knot Point"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(computer.FindProperty("performanceYawGain").FindPropertyRelative("factor"), new GUIContent("Perf Knot Point"));
            }
            GUILayout.Space(8f);
            GUI.color = Color.grey;
            EditorGUILayout.HelpBox("Base Yaw Gain Set", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("baseYawGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("baseYawGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("baseYawGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));


            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Cruise Yaw Gain Set", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("cruiseYawGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("cruiseYawGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("cruiseYawGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));


            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Performance Yaw Gain Set", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("performanceYawGain").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("performanceYawGain").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("performanceYawGain").FindPropertyRelative("Kd"), new GUIContent("Derivative"));


           

            GUILayout.Space(5f);
            GUI.color = Color.grey;
            EditorGUILayout.HelpBox("Current Yaw Gain Set", MessageType.None);
            GUI.color = backgroundColor;
            GUI.enabled = false;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("yawRateSolver").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("yawRateSolver").FindPropertyRelative("Ki"), new GUIContent("Integral"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("yawRateSolver").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("yawRateSolver").FindPropertyRelative("output"), new GUIContent("Ouput"));
            GUI.enabled = true;

            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("yawRateSolver").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(computer.FindProperty("yawRateSolver").FindPropertyRelative("maximum"), new GUIContent("Maximum"));
        }













        GUILayout.Space(20f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Outer-Loop controllers. The output from these controllers is sent as input to the Inner-Loop controller set", MessageType.Info);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);

        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Pitch Angle Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("pitchAngleSolver").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("pitchAngleSolver").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("pitchAngleSolver").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUI.enabled = false;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("pitchAngleSolver").FindPropertyRelative("output"), new GUIContent("Ouput"));
        GUI.enabled = true;

        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(computer.FindProperty("pitchAngleSolver").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("pitchAngleSolver").FindPropertyRelative("maximum"), new GUIContent("Maximum"));


        GUILayout.Space(10f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Roll Angle Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("rollAngleSolver").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("rollAngleSolver").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("rollAngleSolver").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUI.enabled = false;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("rollAngleSolver").FindPropertyRelative("output"), new GUIContent("Ouput"));
        GUI.enabled = true;

        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(computer.FindProperty("rollAngleSolver").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("rollAngleSolver").FindPropertyRelative("maximum"), new GUIContent("Maximum"));


        GUILayout.Space(10f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Yaw Angle Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("yawAngleSolver").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("yawAngleSolver").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("yawAngleSolver").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUI.enabled = false;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("yawAngleSolver").FindPropertyRelative("output"), new GUIContent("Ouput"));
        GUI.enabled = true;

        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(computer.FindProperty("yawAngleSolver").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("yawAngleSolver").FindPropertyRelative("maximum"), new GUIContent("Maximum"));


        GUILayout.Space(10f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Altitude Controller Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("altitudeSolver").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("altitudeSolver").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("altitudeSolver").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUI.enabled = false;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("altitudeSolver").FindPropertyRelative("output"), new GUIContent("Ouput"));
        GUI.enabled = true;

        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(computer.FindProperty("altitudeSolver").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("altitudeSolver").FindPropertyRelative("maximum"), new GUIContent("Maximum"));

        GUILayout.Space(10f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Climb Controller Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("climbSolver").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("climbSolver").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("climbSolver").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUI.enabled = false;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("climbSolver").FindPropertyRelative("output"), new GUIContent("Ouput"));
        GUI.enabled = true;

        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(computer.FindProperty("climbSolver").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("climbSolver").FindPropertyRelative("maximum"), new GUIContent("Maximum"));






        GUILayout.Space(10f);
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Turn Controller Gain", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("turnSolver").FindPropertyRelative("Kp"), new GUIContent("Proportional"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("turnSolver").FindPropertyRelative("Ki"), new GUIContent("Integral"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("turnSolver").FindPropertyRelative("Kd"), new GUIContent("Derivative"));
        GUI.enabled = false;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("turnSolver").FindPropertyRelative("output"), new GUIContent("Ouput"));
        GUI.enabled = true;

        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(computer.FindProperty("turnSolver").FindPropertyRelative("minimum"), new GUIContent("Minimum"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(computer.FindProperty("turnSolver").FindPropertyRelative("maximum"), new GUIContent("Maximum"));


        serializedObject.ApplyModifiedProperties();
        computer.ApplyModifiedProperties();
    }
}
#endif

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


#if UNITY_EDITOR

// --------------------------------------------- TurboFan
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroTurboFan))]
public class SilantroTurboFanEditor : Editor
{
    Color backgroundColor;
    Color silantroColor = new Color(1.0f, 0.40f, 0f);
    SilantroTurboFan jet;
    SerializedProperty core;
    public int toolbarTab;
    public string currentTab;
    public string pitchLabel = "Pitch";


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnEnable() { jet = (SilantroTurboFan)target; core = serializedObject.FindProperty("core"); }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector ();
        serializedObject.Update();

        GUILayout.Space(2f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Identifier", MessageType.None);
        GUI.color = backgroundColor;
        EditorGUILayout.PropertyField(core.FindPropertyRelative("engineIdentifier"), new GUIContent(" "));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("enginePosition"), new GUIContent("Position"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("engineNumber"), new GUIContent("Number"));

        GUILayout.Space(5f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Engine Class", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("engineType"), new GUIContent(" "));


        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Properties", MessageType.None);
        GUI.color = backgroundColor;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("engineDiameter"), new GUIContent("Engine Diameter"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("intakePercentage"), new GUIContent("Intake Ratio"));
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Intake Diameter", jet.diffuserDrawDiameter.ToString("0.000") + " m");

        if (jet.engineType == SilantroTurboFan.EngineType.Mixed)
        {
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("coreExhaustPercentage"), new GUIContent("Exhaust Ratio"));
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Exhaust Diameter", jet.coreExhaustDrawDiameter.ToString("0.000") + " m");
        }

        if (jet.engineType == SilantroTurboFan.EngineType.Unmixed)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fanExhaustPercentage"), new GUIContent("Fan Exhaust Ratio"));
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Fan Exhaust Diameter", jet.fanExhaustDrawDiameter.ToString("0.000") + " m");
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("coreExhaustPercentage"), new GUIContent("Core Exhaust Ratio"));
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Core Exhaust Diameter", jet.coreExhaustDrawDiameter.ToString("0.000") + " m");
        }

        GUILayout.Space(8f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("intakeType"), new GUIContent("Intake Type"));
        if (jet.engineType == SilantroTurboFan.EngineType.Mixed)
        {
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reheatSystem"), new GUIContent("Reheat System"));
        }


        GUILayout.Space(7f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("vectoringType"), new GUIContent("Thrust Vectoring"));
       



        GUILayout.Space(7f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("reverseThrustMode"), new GUIContent("Reverse Thrust"));

        if(jet.reverseThrustMode == SilantroTurboFan.ReverseThrust.Available)
        {
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Reverse Thrust Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Actuation Level", (jet.core.reverseThrustFactor*100f).ToString("0.0") + " %");
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("reverseThrustPercentage"), new GUIContent("Extraction Percentage"));
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Core", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("functionalRPM"), new GUIContent("Intake RPM"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("overspeedAllowance"), new GUIContent("Overspeed Allowance"));
        GUILayout.Space(4f);
        EditorGUILayout.LabelField("Maximum RPM", jet.core.maximumRPM.ToString("0.0") + " RPM");
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Minimum RPM", jet.core.minimumRPM.ToString("0.0") + " RPM");
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("baseCoreAcceleration"), new GUIContent("Core Acceleration"));
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Core RPM", jet.core.coreRPM.ToString("0.0") + " RPM");


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Thermodynamic Properties", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("β"), new GUIContent("By-Pass Ratio"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("πc"), new GUIContent("Core Pressure Ratio"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("πf"), new GUIContent("Fan Pressure Ratio"));
        GUILayout.Space(3f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Pressure Drop (%)", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pcc"), new GUIContent("Compressor"));
        if (jet.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pcab"), new GUIContent("Afterburner Pipe"));
        }

        GUILayout.Space(3f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Turbine Inlet Temperature (°K)", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TIT"), new GUIContent(" "));
        if (jet.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning)
        {
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Maximum Engine Temperature (°K)", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaximumTemperature"), new GUIContent(" "));
        }
        GUILayout.Space(5f);
        EditorGUILayout.LabelField("TSFC ", jet.TSFC.ToString("0.00") + " lb/lbf.hr");

        GUILayout.Space(5f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Efficiency Configuration (%)", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nd"), new GUIContent("Diffuser"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nf"), new GUIContent("Fan"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nc"), new GUIContent("Compressor"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nb"), new GUIContent("Burner"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nhpt"), new GUIContent("Turbine (HPT)"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nlpt"), new GUIContent("Turbine (LPT)"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nn"), new GUIContent("Nozzle"));
        if (jet.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nab"), new GUIContent("Afterburner"));
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Thermodynamic Performance", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("showPerformance"), new GUIContent("Show"));
        if (jet.showPerformance)
        {
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("AMBIENT", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Pa: " + jet.Pa.ToString("0.00") + " KPa" + " || Ta: " + jet.Ta.ToString("0.00") + " °K");
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("DIFFUSER", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("P2: " + jet.P02.ToString("0.00") + " KPa" + " || T2: " + jet.T02.ToString("0.00") + " °K");
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("FAN", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("P3: " + jet.P03.ToString("0.00") + " KPa" + " || T3: " + jet.T03.ToString("0.00") + " °K");
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("COMPRESSOR", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("P4: " + jet.P04.ToString("0.00") + " KPa" + " || T4: " + jet.T04.ToString("0.00") + " °K");
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("BURNER", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("P5: " + jet.P05.ToString("0.00") + " KPa" + " || T5: " + jet.T05.ToString("0.00") + " °K");
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("HIGH PRESSURE TURBINE", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("P6: " + jet.P06.ToString("0.00") + " KPa" + " || T6: " + jet.T06.ToString("0.00") + " °K");
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox(" LOW PRESSURE TURBINE", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("P7: " + jet.P7.ToString("0.00") + " KPa" + " || T7: " + jet.T7.ToString("0.00") + " °K");
            if (!jet.core.afterburnerOperative)
            {
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("NOZZLE", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("P8: " + jet.P08.ToString("0.00") + " KPa" + " || T8: " + jet.T08.ToString("0.00") + " °K");
            }
            else
            {
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("AFTERBURER PIPE", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("P9: " + jet.P10.ToString("0.00") + " KPa" + " || T9: " + jet.T10.ToString("0.00") + " °K");
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("NOZZLE", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(2f);
                float t10_t9 = (jet.T10 / ((jet.γ9 + 1) / 2));
                EditorGUILayout.LabelField("P10: " + (jet.P08 / 1.94f).ToString("0.00") + " KPa" + " || T10: " + t10_t9.ToString("0.00") + " °K");
            }
            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Exhaust Gas Properties", MessageType.None);
            GUI.color = backgroundColor;
            if (jet.engineType == SilantroTurboFan.EngineType.Mixed)
            {
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("Velocity", jet.Ue.ToString("0.00") + " m/s");
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Temperature (EGT)", jet.Te.ToString("0.00") + " °C");
            }
            else
            {
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("Fan Exhaust", jet.Uef.ToString("0.00") + " m/s");
                GUILayout.Space(3f);
                float fEGT = jet.T09 - 273.15f; float cEGT = jet.T08 - 273.15f;
                EditorGUILayout.LabelField("Fan (EGT)", fEGT.ToString("0.00") + " °C");
                //
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Core Exhaust", jet.Ue.ToString("0.00") + " m/s");
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Core (EGT)", cEGT.ToString("0.00") + " °C");
            }
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Flows Rates", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Intake Air", jet.ma.ToString("0.00") + " kg/s");
            if (jet.engineType == SilantroTurboFan.EngineType.Mixed)
            {
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Exhaust gas", jet.Me.ToString("0.00") + " kg/s");
            }
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Fuel", jet.mf.ToString("0.00") + " kg/s");
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
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

      



        if (jet.engineType == SilantroTurboFan.EngineType.Mixed)
        {
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exitPoint"), new GUIContent("Exhaust Point"));
        }
        if (jet.engineType == SilantroTurboFan.EngineType.Unmixed)
        {
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exitPoint"), new GUIContent("Core Exhaust Point"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fanExhaustPoint"), new GUIContent("Fan Exhaust Point"));
        }
        if (jet.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning)
        {
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("dataSource"), new GUIContent("Nozzle Actuator"));
        }

        if (jet.core.vectoringType != SilantroEngineCore.ThrustVectoring.None)
        {
            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Thrust Vectoring Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("nozzlePivot"), new GUIContent("Nozzle Hinge"));
            GUILayout.Space(8f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(core.FindPropertyRelative("pitchLabel"), new GUIContent(""));
            EditorGUILayout.PropertyField(core.FindPropertyRelative("pitchAxis"), new GUIContent(""));
            EditorGUILayout.PropertyField(core.FindPropertyRelative("pitchDirection"), new GUIContent(""));
            EditorGUILayout.PropertyField(core.FindPropertyRelative("maximumPitchDeflection"), new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            if (jet.core.vectoringType == SilantroEngineCore.ThrustVectoring.TwoAxis || jet.core.vectoringType == SilantroEngineCore.ThrustVectoring.ThreeAxis)
            {
                GUILayout.Space(3f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(core.FindPropertyRelative("rollLabel"), new GUIContent(""));
                EditorGUILayout.PropertyField(core.FindPropertyRelative("rollAxis"), new GUIContent(""));
                EditorGUILayout.PropertyField(core.FindPropertyRelative("rollDirection"), new GUIContent(""));
                EditorGUILayout.PropertyField(core.FindPropertyRelative("maximumRollDeflection"), new GUIContent(""));
                EditorGUILayout.EndHorizontal();
            }

            if (jet.core.vectoringType == SilantroEngineCore.ThrustVectoring.ThreeAxis)
            {
                GUILayout.Space(3f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(core.FindPropertyRelative("yawLabel"), new GUIContent(""));
                EditorGUILayout.PropertyField(core.FindPropertyRelative("yawAxis"), new GUIContent(""));
                EditorGUILayout.PropertyField(core.FindPropertyRelative("yawDirection"), new GUIContent(""));
                EditorGUILayout.PropertyField(core.FindPropertyRelative("maximumYawDeflection"), new GUIContent(""));
                EditorGUILayout.EndHorizontal();
            }
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("soundMode"), new GUIContent("Mode"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorMode"), new GUIContent("Cabin Sounds"));
        GUILayout.Space(5f);
        if (jet.core.soundMode == SilantroEngineCore.SoundMode.Basic)
        {
            if (jet.core.interiorMode == SilantroEngineCore.InteriorMode.Off)
            {
                EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Ignition Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Shutdown Sound"));
            }
            else
            {
                toolbarTab = GUILayout.Toolbar(toolbarTab, new string[] { "Exterior Sounds", "Interior Sounds" });
                GUILayout.Space(5f);
                switch (toolbarTab)
                {
                    case 0: currentTab = "Exterior Sounds"; break;
                    case 1: currentTab = "Interior Sounds"; break;
                }
                switch (currentTab)
                {
                    case "Exterior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Ignition Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Shutdown Sound"));
                        break;

                    case "Interior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionInterior"), new GUIContent("Interior Ignition"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorIdle"), new GUIContent("Interior Idle"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownInterior"), new GUIContent("Interior Shutdown"));
                        break;
                }
            }
        }
        else
        {
            GUILayout.Space(3f);
            if (jet.core.interiorMode == SilantroEngineCore.InteriorMode.Off)
            {
                EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Exterior Ignition"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("frontIdle"), new GUIContent("Front Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("sideIdle"), new GUIContent("Side Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Rear Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Exterior Shutdown"));
            }
            else
            {
                toolbarTab = GUILayout.Toolbar(toolbarTab, new string[] { "Exterior Sounds", "Interior Sounds" });
                GUILayout.Space(5f);
                switch (toolbarTab)
                {
                    case 0: currentTab = "Exterior Sounds"; break;
                    case 1: currentTab = "Interior Sounds"; break;
                }
                switch (currentTab)
                {
                    case "Exterior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Exterior Ignition"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("frontIdle"), new GUIContent("Front Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("sideIdle"), new GUIContent("Side Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Rear Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Exterior Shutdown"));
                        break;

                    case "Interior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionInterior"), new GUIContent("Interior Ignition"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorIdle"), new GUIContent("Interior Idle"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownInterior"), new GUIContent("Interior Shutdown"));
                        break;
                }
            }
        }



        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Effects Configuration", MessageType.None);
        GUI.color = backgroundColor;
        EditorGUILayout.PropertyField(core.FindPropertyRelative("baseEffects"), new GUIContent("Use Effects"));
        if(jet.core.baseEffects)
        {
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("exhaustSmoke"), new GUIContent("Exhaust Smoke"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("smokeEmissionLimit"), new GUIContent("Maximum Emission"));

            GUILayout.Space(10f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("baseEmission"), new GUIContent("Core Emission"));
            if(jet.core.baseEmission)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Exhaust Emission Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("engineMaterial"), new GUIContent("Core Material"));

                if (jet.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("afterburnerTubeMaterial"), new GUIContent("Afterburner Pipe Material"));
                }
                else
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("afterburnerTubeMaterial"), new GUIContent("Pipe Material"));
                }

                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("maximumNormalEmission"), new GUIContent("Maximum Emission"));
                GUILayout.Space(2f);
                if (jet.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning)
                {
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("maximumAfterburnerEmission"), new GUIContent("Maximum Afterburner Emission"));
                }
            }



            GUILayout.Space(10f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("coreFlame"), new GUIContent("Flame Effect"));
            if (jet.core.coreFlame)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Exhaust Flame Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("flameType"), new GUIContent("Flame Type"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("flameObject"), new GUIContent("Flame Object"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("flameMaterial"), new GUIContent("Flame Material"));
                

                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Normal Mode", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("normalDiameter"), new GUIContent("Dry Flame Diameter"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("normalLength"), new GUIContent("Dry Flame Length"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("normalAlpha"), new GUIContent("Dry Flame Alpha"));

                if (jet.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning)
                {
                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Afterburner Mode", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("wetDiameter"), new GUIContent("Wet Flame Diameter"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("wetLength"), new GUIContent("Wet Flame Length"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("wetAlpha"), new GUIContent("Wet Flame Alpha"));
                }


                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("alphaSpeed"), new GUIContent("Alpha Speed"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("scaleSpeed"), new GUIContent("Scale Speed"));
            }
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Output", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Core Power", (jet.core.corePower * jet.core.coreFactor * 100f).ToString("0.00") + " %");
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Engine Thrust", jet.engineThrust.ToString("0.0") + " N");


        serializedObject.ApplyModifiedProperties();
    }
}



// --------------------------------------------- TurboJet
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroTurboJet))]
public class SilantroTurboJetEditor : Editor
{

    Color backgroundColor;
    Color silantroColor = new Color(1.0f, 0.40f, 0f);
    SilantroTurboJet jet;
    SerializedProperty core;
    public int toolbarTab;
    public string currentTab;



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnEnable() { jet = (SilantroTurboJet)target; core = serializedObject.FindProperty("core"); }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector ();
        serializedObject.Update();

        GUILayout.Space(2f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Identifier", MessageType.None);
        GUI.color = backgroundColor;
        EditorGUILayout.PropertyField(core.FindPropertyRelative("engineIdentifier"), new GUIContent(" "));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("enginePosition"), new GUIContent("Position"));


        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Properties", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("engineDiameter"), new GUIContent("Engine Diameter"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("intakePercentage"), new GUIContent("Intake Ratio"));
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Intake Diameter", jet.diffuserDrawDiameter.ToString("0.000") + " m");

        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("exhaustPercentage"), new GUIContent("Exhaust Ratio"));
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Exhaust Diameter", jet.exhaustDrawDiameter.ToString("0.000") + " m");

        GUILayout.Space(8f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("intakeType"), new GUIContent("Intake Type"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("reheatSystem"), new GUIContent("Reheat System"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("reverseThrustMode"), new GUIContent("Reverse Thrust"));

        if (jet.reverseThrustMode == SilantroTurboJet.ReverseThrust.Available)
        {
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Reverse Thrust Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Actuation Level", (jet.core.reverseThrustFactor*100f).ToString("0.0") + " %");
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("reverseThrustPercentage"), new GUIContent("Extraction Percentage"));
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Core", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("functionalRPM"), new GUIContent("Intake RPM"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("overspeedAllowance"), new GUIContent("Overspeed Allowance"));
        GUILayout.Space(4f);
        EditorGUILayout.LabelField("Maximum RPM", jet.core.maximumRPM.ToString("0.0") + " RPM");
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Minimum RPM", jet.core.minimumRPM.ToString("0.0") + " RPM");
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("baseCoreAcceleration"), new GUIContent("Core Acceleration"));
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Core RPM", jet.core.coreRPM.ToString("0.0") + " RPM");


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Thermodynamic Properties", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("πc"), new GUIContent("Core Pressure Ratio"));
        GUILayout.Space(3f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Pressure Drop (%)", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pcc"), new GUIContent("Compressor"));
        if (jet.reheatSystem == SilantroTurboJet.ReheatSystem.Afterburning)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pcab"), new GUIContent("Afterburner Pipe"));
        }

        GUILayout.Space(3f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Turbine Inlet Temperature (°K)", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TIT"), new GUIContent(" "));
        if (jet.reheatSystem == SilantroTurboJet.ReheatSystem.Afterburning)
        {
            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Maximum Engine Temperature (°K)", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaximumTemperature"), new GUIContent(" "));
        }
        GUILayout.Space(5f);
        EditorGUILayout.LabelField("TSFC ", jet.TSFC.ToString("0.00") + " lb/lbf.hr");



        GUILayout.Space(5f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Efficiency Configuration (%)", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nd"), new GUIContent("Diffuser"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nc"), new GUIContent("Compressor"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nb"), new GUIContent("Burner"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nt"), new GUIContent("Turbine"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nn"), new GUIContent("Nozzle"));
        if (jet.reheatSystem == SilantroTurboJet.ReheatSystem.Afterburning)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nab"), new GUIContent("Afterburner"));
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
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
       
        if (jet.reheatSystem == SilantroTurboJet.ReheatSystem.Afterburning)
        {
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("dataSource"), new GUIContent("Nozzle Actuator"));
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("soundMode"), new GUIContent("Mode"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorMode"), new GUIContent("Cabin Sounds"));
        GUILayout.Space(5f);
        if (jet.core.soundMode == SilantroEngineCore.SoundMode.Basic)
        {
            if (jet.core.interiorMode == SilantroEngineCore.InteriorMode.Off)
            {
                EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Ignition Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Shutdown Sound"));
            }
            else
            {
                toolbarTab = GUILayout.Toolbar(toolbarTab, new string[] { "Exterior Sounds", "Interior Sounds" });
                GUILayout.Space(5f);
                switch (toolbarTab)
                {
                    case 0: currentTab = "Exterior Sounds"; break;
                    case 1: currentTab = "Interior Sounds"; break;
                }
                switch (currentTab)
                {
                    case "Exterior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Ignition Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Shutdown Sound"));
                        break;

                    case "Interior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionInterior"), new GUIContent("Interior Ignition"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorIdle"), new GUIContent("Interior Idle"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownInterior"), new GUIContent("Interior Shutdown"));
                        break;
                }
            }
        }
        else
        {
            GUILayout.Space(3f);
            if (jet.core.interiorMode == SilantroEngineCore.InteriorMode.Off)
            {
                EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Exterior Ignition"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("frontIdle"), new GUIContent("Front Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("sideIdle"), new GUIContent("Side Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Rear Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Exterior Shutdown"));
            }
            else
            {
                toolbarTab = GUILayout.Toolbar(toolbarTab, new string[] { "Exterior Sounds", "Interior Sounds" });
                GUILayout.Space(5f);
                switch (toolbarTab)
                {
                    case 0: currentTab = "Exterior Sounds"; break;
                    case 1: currentTab = "Interior Sounds"; break;
                }
                switch (currentTab)
                {
                    case "Exterior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Exterior Ignition"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("frontIdle"), new GUIContent("Front Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("sideIdle"), new GUIContent("Side Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Rear Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Exterior Shutdown"));
                        break;

                    case "Interior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionInterior"), new GUIContent("Interior Ignition"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorIdle"), new GUIContent("Interior Idle"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownInterior"), new GUIContent("Interior Shutdown"));
                        break;
                }
            }
        }



        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Effects Configuration", MessageType.None);
        GUI.color = backgroundColor;
        EditorGUILayout.PropertyField(core.FindPropertyRelative("baseEffects"), new GUIContent("Use Effects"));
        if (jet.core.baseEffects)
        {
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("exhaustSmoke"), new GUIContent("Exhaust Smoke"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("smokeEmissionLimit"), new GUIContent("Maximum Emission"));

            GUILayout.Space(10f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("baseEmission"), new GUIContent("Emission Effect"));
            if (jet.core.baseEmission)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Exhaust Emission Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("engineMaterial"), new GUIContent("Core Material"));

                if (jet.reheatSystem == SilantroTurboJet.ReheatSystem.Afterburning)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("afterburnerTubeMaterial"), new GUIContent("Afterburner Pipe Material"));
                }
                else
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("afterburnerTubeMaterial"), new GUIContent("Pipe Material"));
                }

                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("maximumNormalEmission"), new GUIContent("Maximum Emission"));
                GUILayout.Space(2f);
                if (jet.reheatSystem == SilantroTurboJet.ReheatSystem.Afterburning)
                {
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("maximumAfterburnerEmission"), new GUIContent("Maximum Afterburner Emission"));
                }
            }



            GUILayout.Space(10f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("coreFlame"), new GUIContent("Flame Effect"));
            if (jet.core.coreFlame)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Exhaust Flame Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("flameType"), new GUIContent("Flame Type"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("flameObject"), new GUIContent("Flame Object"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("flameMaterial"), new GUIContent("Flame Material"));

                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Normal Mode", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("normalDiameter"), new GUIContent("Dry Flame Diameter"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("normalLength"), new GUIContent("Dry Flame Length"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("normalAlpha"), new GUIContent("Dry Flame Alpha"));

                if (jet.reheatSystem == SilantroTurboJet.ReheatSystem.Afterburning)
                {
                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Afterburner Mode", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("wetDiameter"), new GUIContent("Wet Flame Diameter"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("wetLength"), new GUIContent("Wet Flame Length"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(core.FindPropertyRelative("wetAlpha"), new GUIContent("Wet Flame Alpha"));
                }

                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("alphaSpeed"), new GUIContent("Alpha Speed"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("scaleSpeed"), new GUIContent("Scale Speed"));
            }
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Output", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Core Power", (jet.core.corePower * jet.core.coreFactor * 100f).ToString("0.00") + " %");
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Engine Thrust", jet.engineThrust.ToString("0.0") + " N");


        serializedObject.ApplyModifiedProperties();
    }
}


// --------------------------------------------- TurboProp
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroTurboProp))]
public class SilantroTurboPropEditor : Editor
{

    Color backgroundColor;
    Color silantroColor = new Color(1.0f, 0.40f, 0f);
    SilantroTurboProp prop;
    SerializedProperty core;
    public int toolbarTab;
    public string currentTab;



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnEnable() { prop = (SilantroTurboProp)target; core = serializedObject.FindProperty("core"); }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        DrawDefaultInspector ();
        serializedObject.Update();

        GUILayout.Space(2f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Identifier", MessageType.None);
        GUI.color = backgroundColor;
        EditorGUILayout.PropertyField(core.FindPropertyRelative("engineIdentifier"), new GUIContent(" "));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("enginePosition"), new GUIContent("Position"));


        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Properties", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("engineDiameter"), new GUIContent("Engine Diameter"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("intakePercentage"), new GUIContent("Intake Ratio"));
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Intake Diameter", prop.diffuserDrawDiameter.ToString("0.000") + " m");

        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("exhaustPercentage"), new GUIContent("Exhaust Ratio"));
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Exhaust Diameter", prop.exhaustDrawDiameter.ToString("0.000") + " m");

        GUILayout.Space(8f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("intakeType"), new GUIContent("Intake Type"));


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Core", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("functionalRPM"), new GUIContent("Intake RPM"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("overspeedAllowance"), new GUIContent("Overspeed Allowance"));
        GUILayout.Space(4f);
        EditorGUILayout.LabelField("Maximum RPM", prop.core.maximumRPM.ToString("0.0") + " RPM");
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Minimum RPM", prop.core.minimumRPM.ToString("0.0") + " RPM");
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("baseCoreAcceleration"), new GUIContent("Core Acceleration"));
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Core RPM", prop.core.coreRPM.ToString("0.0") + " RPM");


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Thermodynamic Properties", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("πc"), new GUIContent("Core Pressure Ratio"));
        GUILayout.Space(3f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Pressure Drop (%)", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pcc"), new GUIContent("Compressor"));
        GUILayout.Space(3f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Turbine Inlet Temperature (°K)", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("TIT"), new GUIContent(" "));
        GUILayout.Space(5f);
        EditorGUILayout.LabelField("TSFC ", prop.PSFC.ToString("0.00") + " lb/lbf.hr");



        GUILayout.Space(5f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Efficiency Configuration (%)", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nd"), new GUIContent("Diffuser"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nc"), new GUIContent("Compressor"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nb"), new GUIContent("Burner"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nt"), new GUIContent("Turbine"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nn"), new GUIContent("Nozzle"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ng"), new GUIContent("Gear"));





        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
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
        EditorGUILayout.PropertyField(core.FindPropertyRelative("soundMode"), new GUIContent("Mode"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorMode"), new GUIContent("Cabin Sounds"));
        GUILayout.Space(5f);
        if (prop.core.soundMode == SilantroEngineCore.SoundMode.Basic)
        {
            if (prop.core.interiorMode == SilantroEngineCore.InteriorMode.Off)
            {
                EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Ignition Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Shutdown Sound"));
            }
            else
            {
                toolbarTab = GUILayout.Toolbar(toolbarTab, new string[] { "Exterior Sounds", "Interior Sounds" });
                GUILayout.Space(5f);
                switch (toolbarTab)
                {
                    case 0: currentTab = "Exterior Sounds"; break;
                    case 1: currentTab = "Interior Sounds"; break;
                }
                switch (currentTab)
                {
                    case "Exterior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Ignition Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Shutdown Sound"));
                        break;

                    case "Interior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionInterior"), new GUIContent("Interior Ignition"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorIdle"), new GUIContent("Interior Idle"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownInterior"), new GUIContent("Interior Shutdown"));
                        break;
                }
            }
        }
        else
        {
            GUILayout.Space(3f);
            if (prop.core.interiorMode == SilantroEngineCore.InteriorMode.Off)
            {
                EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Exterior Ignition"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("frontIdle"), new GUIContent("Front Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("sideIdle"), new GUIContent("Side Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Rear Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Exterior Shutdown"));
            }
            else
            {
                toolbarTab = GUILayout.Toolbar(toolbarTab, new string[] { "Exterior Sounds", "Interior Sounds" });
                GUILayout.Space(5f);
                switch (toolbarTab)
                {
                    case 0: currentTab = "Exterior Sounds"; break;
                    case 1: currentTab = "Interior Sounds"; break;
                }
                switch (currentTab)
                {
                    case "Exterior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Exterior Ignition"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("frontIdle"), new GUIContent("Front Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("sideIdle"), new GUIContent("Side Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Rear Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Exterior Shutdown"));
                        break;

                    case "Interior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionInterior"), new GUIContent("Interior Ignition"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorIdle"), new GUIContent("Interior Idle"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownInterior"), new GUIContent("Interior Shutdown"));
                        break;
                }
            }
        }



        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Effects Configuration", MessageType.None);
        GUI.color = backgroundColor;
        EditorGUILayout.PropertyField(core.FindPropertyRelative("baseEffects"), new GUIContent("Use Effects"));
        if (prop.core.baseEffects)
        {
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("exhaustSmoke"), new GUIContent("Exhaust Smoke"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("smokeEmissionLimit"), new GUIContent("Maximum Emission"));
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Output", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Core Power", (prop.core.corePower * prop.core.coreFactor * 100f).ToString("0.00") + " %");
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Brake Power", prop.brakePower.ToString("0.0") + " Hp");


        serializedObject.ApplyModifiedProperties();
    }
}


// --------------------------------------------- Piston
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroPistonEngine))]
public class SilantroPistonEditor : Editor
{

    Color backgroundColor;
    Color silantroColor = new Color(1.0f, 0.40f, 0f);
    SilantroPistonEngine piston;
    SerializedProperty core;
    public int toolbarTab;
    public string currentTab;



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnEnable() { piston = (SilantroPistonEngine)target; core = serializedObject.FindProperty("core"); }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector ();
        serializedObject.Update();

        GUILayout.Space(2f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Identifier", MessageType.None);
        GUI.color = backgroundColor;
        EditorGUILayout.PropertyField(core.FindPropertyRelative("engineIdentifier"), new GUIContent(" "));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("enginePosition"), new GUIContent("Position"));


        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Properties", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("stroke"), new GUIContent("Stroke (In)"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bore"), new GUIContent("Bore (In)"));
        GUILayout.Space(5f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Displacement", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displacement"), new GUIContent(" "));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displacementUnit"), new GUIContent(" "));
       


        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("carburettorType"), new GUIContent("Carburettor Type"));
        GUILayout.Space(10f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("compressionRatio"), new GUIContent("Compression Ratio"));
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("numberOfCylinders"), new GUIContent("No of Cylinders"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("functionalRPM"), new GUIContent("Functional RPM"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("baseCoreAcceleration"), new GUIContent("Core Acceleration"));

        GUILayout.Space(10f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("exitPoint"), new GUIContent("Exhaust Point"));


        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Thermodynamic Properties", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Exhaust Gas Residual (%)", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("residue"), new GUIContent(" "));
        GUILayout.Space(3f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Mechanical Efficiency", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nm"), new GUIContent("Mechanical Efficiency"));
        GUILayout.Space(5f);
        EditorGUILayout.LabelField("Air-Fuel Ratio", piston.AF.ToString("0.00"));
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("PSFC ", piston.PSFC.ToString("0.00") + " lb/hp.hr");

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Thermodynamic Performance", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("showPerformance"), new GUIContent("Show"));
        if (piston.showPerformance)
        {
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Module Performance", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Expansion Work", (piston.W3_4).ToString("0.00") + " kJ");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Compression Work", (piston.W1_2).ToString("0.00") + " kJ");

            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Flows Rates", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Intake Air", piston.ma.ToString("0.00") + " kg/s");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Fuel", piston.Mf.ToString("0.00") + " kg/s");
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("soundMode"), new GUIContent("Mode"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorMode"), new GUIContent("Cabin Sounds"));
        GUILayout.Space(5f);
        if (piston.core.soundMode == SilantroEngineCore.SoundMode.Basic)
        {
            if (piston.core.interiorMode == SilantroEngineCore.InteriorMode.Off)
            {
                EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Ignition Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Shutdown Sound"));
            }
            else
            {
                toolbarTab = GUILayout.Toolbar(toolbarTab, new string[] { "Exterior Sounds", "Interior Sounds" });
                GUILayout.Space(5f);
                switch (toolbarTab)
                {
                    case 0: currentTab = "Exterior Sounds"; break;
                    case 1: currentTab = "Interior Sounds"; break;
                }
                switch (currentTab)
                {
                    case "Exterior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Ignition Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Shutdown Sound"));
                        break;

                    case "Interior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionInterior"), new GUIContent("Interior Ignition"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorIdle"), new GUIContent("Interior Idle"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownInterior"), new GUIContent("Interior Shutdown"));
                        break;
                }
            }
        }
        else
        {
            GUILayout.Space(3f);
            if (piston.core.interiorMode == SilantroEngineCore.InteriorMode.Off)
            {
                EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Exterior Ignition"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("frontIdle"), new GUIContent("Front Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("sideIdle"), new GUIContent("Side Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Rear Idle Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Exterior Shutdown"));
            }
            else
            {
                toolbarTab = GUILayout.Toolbar(toolbarTab, new string[] { "Exterior Sounds", "Interior Sounds" });
                GUILayout.Space(5f);
                switch (toolbarTab)
                {
                    case 0: currentTab = "Exterior Sounds"; break;
                    case 1: currentTab = "Interior Sounds"; break;
                }
                switch (currentTab)
                {
                    case "Exterior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionExterior"), new GUIContent("Exterior Ignition"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("frontIdle"), new GUIContent("Front Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("sideIdle"), new GUIContent("Side Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("backIdle"), new GUIContent("Rear Idle Sound"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownExterior"), new GUIContent("Exterior Shutdown"));
                        break;

                    case "Interior Sounds":
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("ignitionInterior"), new GUIContent("Interior Ignition"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("interiorIdle"), new GUIContent("Interior Idle"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(core.FindPropertyRelative("shutdownInterior"), new GUIContent("Interior Shutdown"));
                        break;
                }
            }
        }



        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Effects Configuration", MessageType.None);
        GUI.color = backgroundColor;
        EditorGUILayout.PropertyField(core.FindPropertyRelative("baseEffects"), new GUIContent("Use Effects"));
        if (piston.core.baseEffects)
        {
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("exhaustSmoke"), new GUIContent("Exhaust Smoke"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(core.FindPropertyRelative("smokeEmissionLimit"), new GUIContent("Maximum Emission"));
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Engine Output", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Core Power", (piston.core.corePower * piston.core.coreFactor * 100f).ToString("0.00") + " %");
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Brake Power", piston.brakePower.ToString("0.0") + " Hp");


        serializedObject.ApplyModifiedProperties();
    }
}


// ---------------------------------------------- Propeller
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroPropeller))]
public class SilantroPropellerEditor : Editor
{

    Color backgroundColor;
    Color silantroColor = new Color(1.0f, 0.40f, 0f);
    SilantroPropeller prop;
    public int toolbarTab;
    public string currentTab;



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnEnable() { prop = (SilantroPropeller)target; }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector();
        serializedObject.Update();

        GUILayout.Space(2f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Power Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("engineType"), new GUIContent("Powerplant"));
        GUILayout.Space(3f);

        if (prop.engineType == SilantroPropeller.EngineType.PistonEngine)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pistonEngine"), new GUIContent(" "));
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Shaft Power", prop.availablePower.ToString("0.00") + " Hp");
        }
        if (prop.engineType == SilantroPropeller.EngineType.TurbopropEngine)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("propEngine"), new GUIContent(" "));
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Shaft Power", prop.availablePower.ToString("0.00") + " Hp");
        }
        if (prop.engineType == SilantroPropeller.EngineType.ElectricMotor)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("electricMotor"), new GUIContent(" "));
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Shaft Power", prop.availablePower.ToString("0.00") + " Hp");
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("thrustPoint"), new GUIContent("Force Point"));
        }



        GUILayout.Space(15f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Blade Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("analysisMethod"), new GUIContent("Analysis Method"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("torqueEffect"), new GUIContent("Torque Effect"));

        if(prop.analysisMethod == SilantroPropeller.AnalysisMethod.InflowModel)
        {
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Dimensions", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Nb"), new GUIContent("No of Blades"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bladeDiameter"), new GUIContent("Blade Diameter"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hubRatio"), new GUIContent("Hub Ratio"));
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Hub Diameter", prop.hubDiameter.ToString("0.00") + " m");
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bladePitch"), new GUIContent("Blade Pitch"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bladePitchAngle"), new GUIContent("Blade Set Angle"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bladeRootChord"), new GUIContent("Blade Root Chord"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bladeTaperRatio"), new GUIContent("Taper Ratio"));
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Tip Chord", prop.tipChord.ToString("0.00") + " m");
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bladeSubdivisions"), new GUIContent("Subdivisions"));

            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bladeAirfoil"), new GUIContent("Blade Airfoil"));
        }
        if (prop.analysisMethod == SilantroPropeller.AnalysisMethod.MomentumTheory)
        {
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bladeDiameter"), new GUIContent("Blade Diameter"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Nb"), new GUIContent("No of Blades"));
        }
        if (prop.analysisMethod == SilantroPropeller.AnalysisMethod.BEM)
        {
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Still in development!", MessageType.Warning);
            GUI.color = backgroundColor;
        }



            GUILayout.Space(15f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Rotation Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Propeller"), new GUIContent("Propeller Transform"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("functionalRPM"), new GUIContent("Rated RPM"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationAxis"), new GUIContent("Rotation Axis"));
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationDirection"), new GUIContent("Rotation Direction"));


        GUILayout.Space(15f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Effect Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("blendMode"), new GUIContent("Blend Mode"));

        if (prop.blendMode != SilantroPropeller.PropellerBlendMode.None)
        {
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("normalBalance"), new GUIContent("Normal Balance"));
        }

        if (prop.blendMode == SilantroPropeller.PropellerBlendMode.Complete)
        {
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Blurred Prop Materials", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            //
            SerializedProperty bmaterials = serializedObject.FindProperty("blurredRotor");
            GUIContent barrelLabel = new GUIContent("Material Count");
            //
            EditorGUILayout.PropertyField(bmaterials.FindPropertyRelative("Array.size"), barrelLabel);
            GUILayout.Space(3f);
            for (int i = 0; i < bmaterials.arraySize; i++)
            {
                GUIContent label = new GUIContent("Material " + (i + 1).ToString());
                EditorGUILayout.PropertyField(bmaterials.GetArrayElementAtIndex(i), label);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            //
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Normal Prop Materials", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            //
            SerializedProperty nmaterials = serializedObject.FindProperty("normalRotor");
            GUIContent nbarrelLabel = new GUIContent("Material Count");
            //
            EditorGUILayout.PropertyField(nmaterials.FindPropertyRelative("Array.size"), nbarrelLabel);
            GUILayout.Space(3f);
            for (int i = 0; i < nmaterials.arraySize; i++)
            {
                GUIContent label = new GUIContent("Material " + (i + 1).ToString());
                EditorGUILayout.PropertyField(nmaterials.GetArrayElementAtIndex(i), label);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        if (prop.blendMode == SilantroPropeller.PropellerBlendMode.Partial)
        {
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Blurred Prop Materials", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            //
            SerializedProperty bmaterials = serializedObject.FindProperty("blurredRotor");
            GUIContent barrelLabel = new GUIContent("Material Count");
            //
            EditorGUILayout.PropertyField(bmaterials.FindPropertyRelative("Array.size"), barrelLabel);
            GUILayout.Space(3f);
            for (int i = 0; i < bmaterials.arraySize; i++)
            {
                GUIContent label = new GUIContent("Material " + (i + 1).ToString());
                EditorGUILayout.PropertyField(bmaterials.GetArrayElementAtIndex(i), label);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }


        // ----------------------------------------------------------------------------------------
        GUILayout.Space(15f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Output", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Thrust", prop.thrust.ToString("0.00") + " N");
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Torque", prop.torque.ToString("0.00") + " Nm");


        serializedObject.ApplyModifiedProperties();
    }
}

#endif
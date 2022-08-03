using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroAerofoil))]
public class SilantroAerofoilEditor : Editor
{
    Color backgroundColor;
    Color silantroColor = new Color(1.0f, 0.40f, 0f);
    SilantroAerofoil aerofoil;



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnEnable() { aerofoil = (SilantroAerofoil)target; }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector();
        serializedObject.Update();



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Aerofoil Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("aerofoilType"), new GUIContent("Type"));
        if(aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing)
        {
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wingType"), new GUIContent("Wing Type"));
        }
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("liftMethod"), new GUIContent("Lift Method"));



        GUILayout.Space(5f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Functionality", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("surfaceFinish"), new GUIContent("Surface Finish"));
        GUILayout.Space(5f);
        if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wingAlignment"), new GUIContent("Alignment"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wingPosition"), new GUIContent(" "));
        }
        else if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Canard)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canardPosition"), new GUIContent("Position"));
        }
        else if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stabOrientation"), new GUIContent("Orientation"));

            if (aerofoil.stabOrientation == SilantroAerofoil.StabilizerOrientation.Horizontal)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilizerPosition"), new GUIContent(" "));
            }
            if (aerofoil.stabOrientation == SilantroAerofoil.StabilizerOrientation.Vertical)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rudderPosition"), new GUIContent(" "));
            }
        }
        else if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilator)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilizerPosition"), new GUIContent(" "));
        }
       

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(10f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Airfoil Component", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(3f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rootAirfoil"), new GUIContent("Root Airfoil"));
        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("tipAirfoil"), new GUIContent("Tip Airfoil"));

        if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing && aerofoil.wingAlignment == SilantroAerofoil.WingAlignment.Monoplane)
        {
            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Vortex Lift Component", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("vortexState"), new GUIContent(" "));

            if (aerofoil.vortexState == SilantroAerofoil.VortexLift.Consider)
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Lift Percentage", (Mathf.Abs(aerofoil.vortexLiftPercentage) * 100f).ToString("0.00") + " %");
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Drag Percentage", (Mathf.Abs(aerofoil.vortexDragPercentage) * 100f).ToString("0.00") + " %");
            }
        }

        if (aerofoil.aerofoilType != SilantroAerofoil.AerofoilType.Balance)
        {
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Ground Effect Component", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("effectState"), new GUIContent(" "));

            if (aerofoil.effectState == SilantroAerofoil.GroundEffectState.Consider)
            {
                GUILayout.Space(10f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("groundInfluenceMethod"), new GUIContent("Analysis Method"));
                GUILayout.Space(5f);
                SerializedProperty layerMask = serializedObject.FindProperty("groundLayer");
                EditorGUILayout.PropertyField(layerMask);

                if (aerofoil.groundInfluenceFactor < 0.99f)
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField("Lift Factor", (1 / (Mathf.Sqrt(aerofoil.groundInfluenceFactor)) * 100f).ToString("0.00") + " %");
                    GUILayout.Space(2f);
                    EditorGUILayout.LabelField("Drag Factor", (aerofoil.groundInfluenceFactor * 100f).ToString("0.00") + " %");
                }
            }



           
            if(aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilator)
            {
                GUILayout.Space(20f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Stabilator Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilatorType"), new GUIContent("Type"));

                if(aerofoil.stabilatorType == SilantroAerofoil.StabilatorType.Coupled)
                {
                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Roll Coupling Percentage", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilatorCouplingPercentage"), new GUIContent(" "));
                }
            }



            if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Canard)
            {
                GUILayout.Space(20f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Canard Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilatorType"), new GUIContent("Type"));

                if (aerofoil.stabilatorType == SilantroAerofoil.StabilatorType.Coupled)
                {
                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Roll Coupling Percentage", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilatorCouplingPercentage"), new GUIContent(" "));
                }
            }


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(20f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Aerofoil Dimensions", MessageType.None); GUI.color = backgroundColor;
            GUILayout.Space(7f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Sweep", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sweepDirection"), new GUIContent("Sweep Direction"));

            if (aerofoil.sweepDirection != SilantroAerofoil.SweepDirection.Unswept)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("aerofoilSweepAngle"), new GUIContent("Sweep Angle"));
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("ɅLE", aerofoil.leadingEdgeSweep.ToString("0.00") + " °");
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Ʌc/4", aerofoil.quaterSweep.ToString("0.00") + " °");
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sweepCorrectionMethod"), new GUIContent("Correction Method"));
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Correction Factor", aerofoil.sweepCorrectionFactor.ToString("0.000"));

            }


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Twist", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("twistDirection"), new GUIContent("Twist Direction"));

            if (aerofoil.twistDirection != SilantroAerofoil.TwistDirection.Untwisted)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("foilTwist"), new GUIContent("Twist Angle"));
            }

            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Structure", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("taperPercentage"), new GUIContent("Taper Percentage"));
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("foilSubdivisions"), new GUIContent("Panel Subdivisions"));





            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tipDesign"), new GUIContent("Tip Design"));
            GUILayout.Space(5f);
            if (aerofoil.tipDesign == SilantroAerofoil.WingtipDesign.Endplate)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("EndPlate Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                serializedObject.FindProperty("wingTipHeight").floatValue = EditorGUILayout.Slider("Plate Height", serializedObject.FindProperty("wingTipHeight").floatValue, 0, aerofoil.foilSpan * 0.5f);
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("wingTipBend"), new GUIContent("Winglet Deflection"));
                aerofoil.wingTipSweep = 0f;
            }
            if (aerofoil.tipDesign == SilantroAerofoil.WingtipDesign.Winglet)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Winglet Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                aerofoil.wingTipHeight = EditorGUILayout.Slider("Winglet Height", aerofoil.wingTipHeight, 0, aerofoil.foilSpan * 0.5f);
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("wingTipBend"), new GUIContent("Winglet Deflection"));
                GUILayout.Space(3f);
                serializedObject.FindProperty("tipSweep").floatValue = EditorGUILayout.Slider("Winglet Sweep", serializedObject.FindProperty("tipSweep").floatValue, 0, 50);
            }
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("ΔAR", aerofoil.effectiveChange.ToString("0.000"));
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(25f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Dimensions", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(1f);
        EditorGUILayout.LabelField("Root Chord", aerofoil.foilRootChord.ToString("0.00") + " m");
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Tip Chord", aerofoil.foilTipChord.ToString("0.00") + " m");
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Aspect Ratio", aerofoil.aspectRatio.ToString("0.000"));
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Oswald Efficiency", (aerofoil.spanEfficiency * 100f).ToString("0.00") + " %");
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Surface Area", aerofoil.foilArea.ToString("0.00") + " m2");
        GUILayout.Space(3f);
        EditorGUILayout.LabelField("Wetted Area", aerofoil.foilWettedArea.ToString("0.00") + " m2");


        if (aerofoil.aerofoilType != SilantroAerofoil.AerofoilType.Balance)
        {
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Draw Options", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("drawFoils"), new GUIContent("Draw Foils"));

            if (aerofoil.drawFoils)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("drawSplits"), new GUIContent("Draw Rib Splits"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("drawMesh"), new GUIContent("Draw Panel Mesh"));
            }
          


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer && aerofoil.stabOrientation == SilantroAerofoil.StabilizerOrientation.Horizontal)
            {
                GUILayout.Space(20f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Stabilizer Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilizerType"), new GUIContent("Type"));

                if (aerofoil.stabilizerType == SilantroAerofoil.StabilizerType.Trimmable)
                {
                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Trim Settings", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("positiveStabLimit"), new GUIContent("Positive Limit"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("negativeStabLimit"), new GUIContent("Negative Limit"));
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("Current Deflection", aerofoil.stabilizerDeflection.ToString("0.00") + " °");

                    GUILayout.Space(10f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Model Configuration", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal();
                    serializedObject.FindProperty("stabilizerModel").objectReferenceValue = EditorGUILayout.ObjectField(serializedObject.FindProperty("controlSurfaceModel").objectReferenceValue, typeof(Transform), true) as Transform;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilizerDeflectionAxis"), new GUIContent(""));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilizerDeflectionDirection"), new GUIContent(""));
                    EditorGUILayout.EndHorizontal();
                }
            }
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            GUILayout.Space(25f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Controls", MessageType.None); GUI.color = backgroundColor;
            GUILayout.Space(7f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("controlState"), new GUIContent("State"));
        }

        if (aerofoil.controlState == SilantroAerofoil.ControlType.Controllable)
        {
            GUILayout.Space(5f);
            if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing) { EditorGUILayout.PropertyField(serializedObject.FindProperty("availableControls"), new GUIContent(" ")); }
            else { aerofoil.availableControls = SilantroAerofoil.AvailableControls.PrimaryOnly; }


            if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing && aerofoil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Available Secondary Surfaces", MessageType.None); GUI.color = backgroundColor;
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("flapState"), new GUIContent("Flaps"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("slatState"), new GUIContent("Slats"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("spoilerState"), new GUIContent("Spoiler"));
            }

            // ---------------------------------------------------------PRIMARY ONLY-------------------------------------------------------------------------------------------------
            if (aerofoil.availableControls == SilantroAerofoil.AvailableControls.PrimaryOnly)
            {
                aerofoil.toolbarStrings = new List<string>();
                if (aerofoil.toolbarStrings.Count == 1) { if (aerofoil.toolbarTab == 1 || aerofoil.toolbarTab > 1) { aerofoil.toolbarTab = 0; } }
                if (!aerofoil.toolbarStrings.Contains("Primary")) { aerofoil.toolbarStrings.Add("Primary"); }
                switch (aerofoil.toolbarTab)
                {
                    case 0: aerofoil.currentTab = aerofoil.toolbarStrings[0]; break;
                }
            }


            // ---------------------------------------------------------MIXED ONLY-------------------------------------------------------------------------------------------------
            if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing && aerofoil.availableControls == SilantroAerofoil.AvailableControls.PrimaryPlusSecondary)
            {
                aerofoil.toolbarStrings = new List<string>();
                if (aerofoil.toolbarStrings.Count == 1) { if (aerofoil.toolbarTab == 1 || aerofoil.toolbarTab > 1) { aerofoil.toolbarTab = 0; } }
                if (!aerofoil.toolbarStrings.Contains("Primary")) { aerofoil.toolbarStrings.Add("Primary"); }
                if (aerofoil.flapState == SilantroAerofoil.ControlState.Active && !aerofoil.toolbarStrings.Contains("Flap")) { aerofoil.toolbarStrings.Add("Flap"); }
                if (aerofoil.slatState == SilantroAerofoil.ControlState.Active && !aerofoil.toolbarStrings.Contains("Slat")) { aerofoil.toolbarStrings.Add("Slat"); }
                if (aerofoil.spoilerState == SilantroAerofoil.ControlState.Active && !aerofoil.toolbarStrings.Contains("Spoiler")) { aerofoil.toolbarStrings.Add("Spoiler"); }
                GUILayout.Space(5f);
                aerofoil.toolbarTab = GUILayout.Toolbar(aerofoil.toolbarTab, aerofoil.toolbarStrings.ToArray());
                //REFRESH IF VALUE IS NULL
                if (!aerofoil.toolbarStrings.Contains(aerofoil.currentTab)) { aerofoil.toolbarTab = 0; }
                //SWITCH TABS
                switch (aerofoil.toolbarTab)
                {
                    case 0: aerofoil.currentTab = aerofoil.toolbarStrings[0]; break;
                    case 1: aerofoil.currentTab = aerofoil.toolbarStrings[1]; break;
                    case 2: aerofoil.currentTab = aerofoil.toolbarStrings[2]; break;
                    case 3: aerofoil.currentTab = aerofoil.toolbarStrings[3]; break;
                }
            }



            // ---------------------------------------------------------SECONDARY ONLY-------------------------------------------------------------------------------------------------
            if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing && aerofoil.availableControls == SilantroAerofoil.AvailableControls.SecondaryOnly)
            {
                aerofoil.toolbarStrings = new List<string>();
                if (aerofoil.toolbarStrings.Count == 1) { if (aerofoil.toolbarTab == 1 || aerofoil.toolbarTab > 1) { aerofoil.toolbarTab = 0; } }
                if (aerofoil.flapState == SilantroAerofoil.ControlState.Active && !aerofoil.toolbarStrings.Contains("Flap")) { aerofoil.toolbarStrings.Add("Flap"); }
                if (aerofoil.slatState == SilantroAerofoil.ControlState.Active && !aerofoil.toolbarStrings.Contains("Slat")) { aerofoil.toolbarStrings.Add("Slat"); }
                if (aerofoil.spoilerState == SilantroAerofoil.ControlState.Active && !aerofoil.toolbarStrings.Contains("Spoiler")) { aerofoil.toolbarStrings.Add("Spoiler"); }


                GUILayout.Space(5f);
                aerofoil.toolbarTab = GUILayout.Toolbar(aerofoil.toolbarTab, aerofoil.toolbarStrings.ToArray());

                //REFRESH IF VALUE IS NULL
                if (!aerofoil.toolbarStrings.Contains(aerofoil.currentTab)) { aerofoil.toolbarTab = 0; }
               
                if (aerofoil.toolbarStrings.Count < 1) { aerofoil.currentTab = " "; aerofoil.toolbarTab = 0; }
                if (aerofoil.toolbarStrings.Count > 0)
                {
                    //SWITCH TABS
                    switch (aerofoil.toolbarTab)
                    {
                        case 0: aerofoil.currentTab = aerofoil.toolbarStrings[0]; break;
                        case 1: aerofoil.currentTab = aerofoil.toolbarStrings[1]; break;
                        case 2: aerofoil.currentTab = aerofoil.toolbarStrings[2]; break;
                    }
                }
            }


            GUILayout.Space(5f);
            switch (aerofoil.currentTab)
            {
                // ---------------------------------------------------------Primary Controls
                case "Primary":
                    if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.SecondaryOnly)
                    {
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Primary", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("surfaceType"), new GUIContent("Surface Type"));
                      

                        if (aerofoil.surfaceType != SilantroAerofoil.SurfaceType.Inactive)
                        {
                            GUILayout.Space(5f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("controlAnalysis"), new GUIContent("Analysis Method"));

                            if(aerofoil.controlAnalysis != SilantroAerofoil.AnalysisMethod.GeometricOnly)
                            {
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("controlCorrectionMethod"), new GUIContent("Numeric Correction"));
                            }

                            if (aerofoil.surfaceType == SilantroAerofoil.SurfaceType.Aileron)
                            {
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("aileronCouple"), new GUIContent("TCS Mode"));
                            }
                            GUILayout.Space(3f);
                            GUI.color = aerofoil.controlColor;
                            EditorGUILayout.HelpBox("Control Chord Ratios (xc/c)", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(5f);
                            GUI.color = backgroundColor;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("controlRootChord"), new GUIContent(aerofoil.surfaceType.ToString() + " Root Chord"));
                            GUILayout.Space(5f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("controlTipChord"), new GUIContent(aerofoil.surfaceType.ToString() + " Tip Chord"));
                            GUILayout.Space(5f);
                            EditorGUILayout.LabelField(aerofoil.surfaceType.ToString() + " Area", aerofoil.controlArea.ToString("0.000") + " m2");
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField(aerofoil.surfaceType.ToString() + " Span", aerofoil.controlSpan.ToString("0.000") + " m");
                            GUILayout.Space(10f);
                            GUI.color = aerofoil.controlColor;
                            EditorGUILayout.HelpBox(aerofoil.surfaceType.ToString() + " Panels", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(5f);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.BeginVertical();
                            SerializedProperty boolsControl = serializedObject.FindProperty("controlSections");
                            for (int ci = 0; ci < boolsControl.arraySize; ci++)
                            {
                                GUIContent labelControl = new GUIContent();
                                if (ci == 0)
                                {
                                    labelControl = new GUIContent("Root Panel: ");
                                }
                                else if (ci == boolsControl.arraySize - 1)
                                {
                                    labelControl = new GUIContent("Tip Panel: ");
                                }
                                else
                                {
                                    labelControl = new GUIContent("Panel: " + (ci + 1).ToString());
                                }
                                EditorGUILayout.PropertyField(boolsControl.GetArrayElementAtIndex(ci), labelControl);
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();

                            GUILayout.Space(10f);
                            GUI.color = aerofoil.controlColor;
                            EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("deflectionType"), new GUIContent("Deflection Type"));

                            if (aerofoil.deflectionType == SilantroAerofoil.DeflectionType.Symmetric)
                            {
                                GUILayout.Space(5f);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("positiveLimit"), new GUIContent("Deflection Limit"));
                            }
                            else
                            {
                                GUILayout.Space(5f);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("positiveLimit"), new GUIContent("Positive Deflection Limit"));
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("negativeLimit"), new GUIContent("Negative Deflection Limit"));
                            }
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Current Deflection", aerofoil.controlDeflection.ToString("0.00") + " °");

                            GUILayout.Space(5f);
                            GUI.color = aerofoil.controlColor;
                            EditorGUILayout.HelpBox("Actuator Configuration", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(5f);
                            GUI.color = Color.white;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("controlActuationSpeed"), new GUIContent("Actuation Speed (°/s)"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumControlTorque"), new GUIContent("Torque Limit (Nm)"));
                            GUILayout.Space(3f);
                            float currentEff = (aerofoil.currentControlEfficiency * 100f);
                            currentEff = Mathf.Clamp(currentEff, 0, 100f);
                            EditorGUILayout.LabelField("Current Efficiency", currentEff.ToString("0.0") + " %");
                            GUILayout.Space(3f);
                            EditorGUILayout.CurveField("Efficiency Curve", aerofoil.controlEfficiencyCurve);
                            GUILayout.Space(3f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Speed Limits", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            float machLock = aerofoil.controlLockPoint / 343f;
                            EditorGUILayout.LabelField("Lock-Up Speed", (aerofoil.controlLockPoint * 1.944f).ToString("0.0") + " knots" + " (M" + machLock.ToString("0.00") + ")");
                            float zeroPoint = aerofoil.controlNullPoint / 343f;
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Null Speed", (aerofoil.controlNullPoint * 1.944f).ToString("0.0") + " knots" + " (M" + zeroPoint.ToString("0.00") + ")");


                            GUILayout.Space(15f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Trim Configuration", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("trimState"), new GUIContent("Trim State"));

                            if (aerofoil.trimState == SilantroAerofoil.TrimState.Available)
                            {
                                GUILayout.Space(3f);
                                //aerofoil.maximumPitchTrim = EditorGUILayout.Slider("Deflection Limit (%)", aerofoil.maximumPitchTrim, 5f, 50f);
                                GUILayout.Space(3f);
                                //aerofoil.trimSpeed = EditorGUILayout.Slider("Wheel Speed", aerofoil.trimSpeed, 0.035f, 0.095f);
                            }


                            GUILayout.Space(10f);
                            GUI.color = aerofoil.controlColor;
                            EditorGUILayout.HelpBox("Model Configuration", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(5f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("baseSurfaceType"), new GUIContent("Model Type"));
                            GUILayout.Space(3f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Base Model", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("controlSurfaceModel"), new GUIContent(""));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("controlDeflectionAxis"), new GUIContent(""));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("controlDeflectionDirection"), new GUIContent(""));
                            EditorGUILayout.EndHorizontal();

                            if(aerofoil.baseSurfaceType == SilantroAerofoil.SurfaceModelType.Dual)
                            {
                                GUILayout.Space(3f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Support Model", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("dualControlSurfaceModel"), new GUIContent(""));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("dualControlDeflectionAxis"), new GUIContent(""));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("dualControlDirection"), new GUIContent(""));
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                     }
                    break;


                // ---------------------------------------------------------Flap Controls
                case "Flap":
                    if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly && aerofoil.flapState == SilantroAerofoil.ControlState.Active)
                    {
                        GUI.color = Color.yellow;
                        EditorGUILayout.HelpBox("Flap", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flapType"), new GUIContent("Flap Type"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flapPosition"), new GUIContent("Position"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flapAnalysis"), new GUIContent("Analysis Method"));
                        if (aerofoil.flapAnalysis != SilantroAerofoil.AnalysisMethod.GeometricOnly)
                        {
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flapCorrectionMethod"), new GUIContent("Numeric Correction"));
                        }


                        GUILayout.Space(5f);
                        GUI.color = Color.yellow;
                        EditorGUILayout.HelpBox("Flap Chord Ratios (xf/c)", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        GUI.color = backgroundColor;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flapRootChord"), new GUIContent("Flap Root Chord :"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flapTipChord"), new GUIContent("Flap Tip Chord :"));

                        GUILayout.Space(5f);
                        EditorGUILayout.LabelField("Flap Area", aerofoil.flapArea.ToString("0.000") + " m2");
                        GUILayout.Space(3f);
                        EditorGUILayout.LabelField("Flap Span", aerofoil.flapSpan.ToString("0.000") + " m");
                        GUILayout.Space(10f);
                        GUI.color = Color.yellow;
                        EditorGUILayout.HelpBox("Flap Panels", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        SerializedProperty boolsflap = serializedObject.FindProperty("flapSections");
                        for (int i = 0; i < boolsflap.arraySize; i++)
                        {
                            if (aerofoil.controlSections[i] != true)
                            {
                                GUIContent labelFlap = new GUIContent();
                                if (i == 0)
                                {
                                    labelFlap = new GUIContent("Root Panel: ");
                                }
                                else if (i == boolsflap.arraySize - 1)
                                {
                                    labelFlap = new GUIContent("Tip Panel: ");
                                }
                                else
                                {
                                    labelFlap = new GUIContent("Panel: " + (i + 1).ToString());
                                }
                                EditorGUILayout.PropertyField(boolsflap.GetArrayElementAtIndex(i), labelFlap);
                            }
                            else
                            {
                                if (aerofoil.surfaceType != SilantroAerofoil.SurfaceType.Inactive)
                                {
                                    string labelFlapNote;
                                    if (i == 0) { labelFlapNote = ("Root Panel: "); }
                                    else if (i == boolsflap.arraySize - 1) { labelFlapNote = ("Tip Panel: "); }
                                    else { labelFlapNote = ("Panel: " + (i + 1).ToString());  }
                                    EditorGUILayout.LabelField(labelFlapNote, aerofoil.surfaceType.ToString());
                                }

                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();

                        GUILayout.Space(10f);
                        GUI.color = Color.yellow;
                        EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Base Flap Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flapAngleSetting"), new GUIContent("Angle Setting"));
                        GUILayout.Space(5f);

                        if (aerofoil.flapAngleSetting == SilantroAerofoil.FlapAngleSetting.FiveStep)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flapA1"), new GUIContent("Flap Level 1"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("takeOffFlap"), new GUIContent("Flap Level 2"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flapA2"), new GUIContent("Flap Level 3"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flapLimit"), new GUIContent("Flaps Full"));
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("takeOffFlap"), new GUIContent("Takeoff Flap"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flapLimit"), new GUIContent("Landing Flap"));
                        }

                        // ----------------------------------------------------------------- Flaperon
                        if (aerofoil.flapType == SilantroAerofoil.FlapType.Flaperon || aerofoil.flapType == SilantroAerofoil.FlapType.Flapevon)
                        {
                            GUILayout.Space(5f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox(aerofoil.flapType.ToString() + " Configuration", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(5f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("positiveFlaperonLimit"), new GUIContent("Positive Deflection Limit"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("negativeFlaperonLimit"), new GUIContent("Negative Deflection Limit"));

                        }

                        GUILayout.Space(3f);
                        EditorGUILayout.LabelField("Surface Deflection", aerofoil.flapDeflection.ToString("0.00") + " °");

                        GUILayout.Space(10f);
                        GUI.color = Color.yellow;
                        EditorGUILayout.HelpBox("Actuator Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flapActuationSpeed"), new GUIContent("Actuation Speed (°/s)"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumFlapTorque"), new GUIContent("Torque Limit (Nm)"));
                        GUILayout.Space(3f);
                        float currentEff = (aerofoil.currentFlapEfficiency * 100f); currentEff = Mathf.Clamp(currentEff, 0, 100f);
                        EditorGUILayout.LabelField("Current Efficiency", currentEff.ToString("0.0") + " %");
                        GUILayout.Space(3f);
                        EditorGUILayout.CurveField("Efficiency Curve", aerofoil.flapEfficiencyCurve);
                        GUILayout.Space(3f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Speed Limits", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        float machLock = aerofoil.flapLockPoint / 343f;
                        EditorGUILayout.LabelField("Lock-Up Speed", (aerofoil.flapLockPoint * 1.944f).ToString("0.0") + " knots" + " (M" + machLock.ToString("0.00") + ")");
                        GUILayout.Space(3f);
                        float zeroPoint = aerofoil.flapNullPoint / 343f;
                        EditorGUILayout.LabelField("Null Speed", (aerofoil.flapNullPoint * 1.944f).ToString("0.0") + " knots" + " (M" + zeroPoint.ToString("0.00") + ")");

                        GUILayout.Space(10f);
                        GUI.color = Color.yellow;
                        EditorGUILayout.HelpBox("Model Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);

                        if (aerofoil.flapType != SilantroAerofoil.FlapType.Flapevon && aerofoil.flapType != SilantroAerofoil.FlapType.Flaperon)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flapModelType"), new GUIContent("Movement Type"));
                            GUILayout.Space(3f);

                            if (aerofoil.flapModelType == SilantroAerofoil.ModelType.Internal)
                            {
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Model Properties", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(5f);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("flapSurfaceType"), new GUIContent("Model Type"));
                                GUILayout.Space(3f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Base Model", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("flapSurfaceModel"), new GUIContent(""));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("flapDeflectionAxis"), new GUIContent(""));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("flapDeflectionDirection"), new GUIContent(""));
                                EditorGUILayout.EndHorizontal();


                                if (aerofoil.flapSurfaceType == SilantroAerofoil.SurfaceModelType.Dual)
                                {
                                    GUILayout.Space(3f);
                                    GUI.color = Color.white;
                                    EditorGUILayout.HelpBox("Support Model", MessageType.None);
                                    GUI.color = backgroundColor;
                                    GUILayout.Space(3f);
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dualFlapSurfaceModel"), new GUIContent(""));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dualFlapDeflectionAxis"), new GUIContent(""));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dualFlapDirection"), new GUIContent(""));
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                            if (aerofoil.flapModelType == SilantroAerofoil.ModelType.Actuator)
                            {
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Actuator Properties", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("flapActuator"), new GUIContent("Flap Actuator"));
                                GUILayout.Space(3f);
                                EditorGUILayout.LabelField("Actuation Level", (aerofoil.flapLevel * 100f).ToString("0.0") + " %");
                            }
                        }
                        else
                        {
                            GUILayout.Space(5f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flapSurfaceType"), new GUIContent("Model Type"));
                            GUILayout.Space(3f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Base Model", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flapSurfaceModel"), new GUIContent(""));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flapDeflectionAxis"), new GUIContent(""));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flapDeflectionDirection"), new GUIContent(""));
                            EditorGUILayout.EndHorizontal();


                            if (aerofoil.flapSurfaceType == SilantroAerofoil.SurfaceModelType.Dual)
                            {
                                GUILayout.Space(3f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Support Model", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("dualFlapSurfaceModel"), new GUIContent(""));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("dualFlapDeflectionAxis"), new GUIContent(""));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("dualFlapDirection"), new GUIContent(""));
                                EditorGUILayout.EndHorizontal();
                            }
                        }



                        GUILayout.Space(10f);
                        GUI.color = Color.yellow;
                        EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flapLoop"), new GUIContent("Loop Sound"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flapClamp"), new GUIContent("Lock Sound"));

                    }
                    break;

                // ---------------------------------------------------------Slat Controls
                case "Slat":
                    if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly && aerofoil.slatState == SilantroAerofoil.ControlState.Active)
                    {
                        GUI.color = Color.magenta;
                        EditorGUILayout.HelpBox("Slat Chord Ratios (xs/c)", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        GUI.color = backgroundColor;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("slatRootChord"), new GUIContent("Slat Root Chord :"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("slatTipChord"), new GUIContent("Slat Tip Chord :"));


                        GUILayout.Space(5f);
                        EditorGUILayout.LabelField("Slat Area", aerofoil.slatArea.ToString("0.000") + " m2");
                        GUILayout.Space(3f);
                        EditorGUILayout.LabelField("Slat Span", aerofoil.slatSpan.ToString("0.000") + " m");

                        GUILayout.Space(10f);
                        GUI.color = Color.magenta;
                        EditorGUILayout.HelpBox("Slat Panels", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        SerializedProperty boolsSlat = serializedObject.FindProperty("slatSections");
                        for (int i = 0; i < boolsSlat.arraySize; i++)
                        {
                            GUIContent labelSlat = new GUIContent();
                            if (i == 0)
                            {
                                labelSlat = new GUIContent("Root Panel: ");
                            }
                            else if (i == boolsSlat.arraySize - 1)
                            {
                                labelSlat = new GUIContent("Tip Panel: ");
                            }
                            else
                            {
                                labelSlat = new GUIContent("Panel: " + (i + 1).ToString());
                            }
                            EditorGUILayout.PropertyField(boolsSlat.GetArrayElementAtIndex(i), labelSlat);
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(10f);
                        GUI.color = Color.magenta;
                        EditorGUILayout.HelpBox("Movement Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("slatMovement"), new GUIContent("Movement Type"));


                        if (aerofoil.slatMovement == SilantroAerofoil.SlatMovement.Deflection)
                        {
                            GUILayout.Space(6f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Deflection Settings", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(4f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumSlatDeflection"), new GUIContent("Deflection Limit  °"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("slatActuationSpeed"), new GUIContent("Actuation Speed  (°/s)"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Current Deflection", aerofoil.slatDeflection.ToString("0.00") + " °");
                        }
                        //SLIDING
                        if (aerofoil.slatMovement == SilantroAerofoil.SlatMovement.Extension)
                        {
                            GUILayout.Space(6f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Extension Settings", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(4f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumSlatDeflection"), new GUIContent("Extension Limit (cm)"));
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("slatActuationSpeed"), new GUIContent("Actuation Speed  (°/s)"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Current Extension", (aerofoil.slatPosition).ToString("0.00") + " cm");
                        }



                        GUILayout.Space(10f);
                        GUI.color = Color.magenta;
                        EditorGUILayout.HelpBox("Model Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("slatModelType"), new GUIContent("Movement Type"));
                        GUILayout.Space(3f);
                        if (aerofoil.slatModelType == SilantroAerofoil.ModelType.Internal)
                        {
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Model Properties", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(5f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("slatSurfaceType"), new GUIContent("Model Type"));
                            GUILayout.Space(3f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Base Model", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("slatSurfaceModel"), new GUIContent(""));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("slatActuationAxis"), new GUIContent(""));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("slatDeflectionDirection"), new GUIContent(""));
                            EditorGUILayout.EndHorizontal();


                            if (aerofoil.slatSurfaceType == SilantroAerofoil.SurfaceModelType.Dual)
                            {
                                GUILayout.Space(3f);
                                GUI.color = Color.white;
                                EditorGUILayout.HelpBox("Support Model", MessageType.None);
                                GUI.color = backgroundColor;
                                GUILayout.Space(3f);
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("dualSlatSurfaceModel"), new GUIContent(""));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("dualSlatActuationAxis"), new GUIContent(""));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("dualSlatDirection"), new GUIContent(""));
                                EditorGUILayout.EndHorizontal();
                            }

                        }
                        if (aerofoil.slatModelType == SilantroAerofoil.ModelType.Actuator)
                        {

                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Actuator Properties", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("slatActuator"), new GUIContent("Slat Actuator"));
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Actuation Level", (aerofoil.slatLevel*100f).ToString("0.0") + " %");
                        }

                    }
                    break;



                // ---------------------------------------------------------Spoiler Controls
                case "Spoiler":
                    if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly && aerofoil.spoilerState == SilantroAerofoil.ControlState.Active)
                    {
                        GUI.color = Color.cyan;
                        EditorGUILayout.HelpBox("Spoiler", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("spoilerType"), new GUIContent("Spoiler Type"));
                        GUILayout.Space(3f);
                        GUI.color = Color.cyan;
                        EditorGUILayout.HelpBox("Spoiler Dimensions", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        GUI.color = backgroundColor;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("spoilerChordFactor"), new GUIContent("Spoiler Chord (xst/c):"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("spoilerHinge"), new GUIContent("Spoiler Hinge: "));
                        if (aerofoil.spoilerType == SilantroAerofoil.SpoilerType.Spoileron)
                        {
                            GUILayout.Space(3f);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("spoilerRollCoupling"), new GUIContent("Roll Coupling %"));
                        }
                        GUILayout.Space(5f);
                        EditorGUILayout.LabelField("Spoiler Area", aerofoil.spoilerArea.ToString("0.000") + " m2");
                        GUILayout.Space(3f);
                        EditorGUILayout.LabelField("Spoiler Span", aerofoil.spoilerSpan.ToString("0.000") + " m");

                        GUILayout.Space(10f);
                        GUI.color = Color.cyan;
                        EditorGUILayout.HelpBox("Spoiler Panels", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        SerializedProperty boolspoilers = serializedObject.FindProperty("spoilerSections");
                        for (int i = 0; i < boolspoilers.arraySize; i++)
                        {
                            GUIContent labelSpoiler = new GUIContent();
                            if (i == 0)
                            {
                                labelSpoiler = new GUIContent("Root Panel: ");
                            }
                            else if (i == boolspoilers.arraySize - 1)
                            {
                                labelSpoiler = new GUIContent("Tip Panel: ");
                            }
                            else
                            {
                                labelSpoiler = new GUIContent("Panel: " + (i + 1).ToString());
                            }
                            EditorGUILayout.PropertyField(boolspoilers.GetArrayElementAtIndex(i), labelSpoiler);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();

                        GUILayout.Space(10f);
                        GUI.color = Color.cyan;
                        EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumSpoilerDeflection"), new GUIContent("Deflection Limit"));
                        GUILayout.Space(5f);
                        EditorGUILayout.LabelField("Spoiler Deflection", aerofoil.spoilerDeflection.ToString("0.00") + " °");

                        GUILayout.Space(5f);
                        GUI.color = Color.cyan;
                        EditorGUILayout.HelpBox("Actuator Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("spoilerActuationSpeed"), new GUIContent("Actuation Speed (°/s)"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumSpoilerTorque"), new GUIContent("Torque Limit (Nm)"));
                        GUILayout.Space(3f);
                        float currentEff = (aerofoil.currentSpoilerEfficiency * 100f); currentEff = Mathf.Clamp(currentEff, 0, 100f);
                        EditorGUILayout.LabelField("Current Efficiency", currentEff.ToString("0.0") + " %");
                        GUILayout.Space(3f);
                        EditorGUILayout.CurveField("Efficiency Curve", aerofoil.spoilerEfficiencyCurve);
                        GUILayout.Space(3f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Speed Limits", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        float machLock = aerofoil.spoilerLockPoint / 343f;
                        EditorGUILayout.LabelField("Lock-Up Speed", (aerofoil.spoilerLockPoint * 1.944f).ToString("0.0") + " knots" + " (M" + machLock.ToString("0.00") + ")");
                        GUILayout.Space(3f);
                        float zeroPoint = aerofoil.spoilerNullPoint / 343f;
                        EditorGUILayout.LabelField("Null Speed", (aerofoil.spoilerNullPoint * 1.944f).ToString("0.0") + " knots" + " (M" + zeroPoint.ToString("0.00") + ")");

                        GUILayout.Space(5f);
                        GUI.color = Color.cyan;
                        EditorGUILayout.HelpBox("Model Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("spoilerSurfaceType"), new GUIContent("Model Type"));
                        GUILayout.Space(3f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Base Model", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("spoilerSurfaceModel"), new GUIContent(""));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("spoilerDeflectionAxis"), new GUIContent(""));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("spoilerDeflectionDirection"), new GUIContent(""));
                        EditorGUILayout.EndHorizontal();

                        if (aerofoil.spoilerSurfaceType == SilantroAerofoil.SurfaceModelType.Dual)
                        {
                            GUILayout.Space(3f);
                            GUI.color = Color.white;
                            EditorGUILayout.HelpBox("Support Model", MessageType.None);
                            GUI.color = backgroundColor;
                            GUILayout.Space(3f);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("dualSpoilerSurfaceModel"), new GUIContent(""));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("dualSpoilerDeflectionAxis"), new GUIContent(""));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("dualSpoilerDirection"), new GUIContent(""));
                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    break;
            }
        }


        


        GUILayout.Space(40f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Output Data", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Root α", aerofoil.rootAlpha.ToString("0.0") + " °");
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Tip α", aerofoil.tipAlpha.ToString("0.0") + " °");
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Lift", aerofoil.TotalLift.ToString("0.0") + " N");
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Induced Drag", aerofoil.TotalBaseDrag.ToString("0.0") + " N");
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("Parasite Drag", aerofoil.TotalSkinDrag.ToString("0.0") + " N");
       

        serializedObject.ApplyModifiedProperties();
    } 
}
#endif
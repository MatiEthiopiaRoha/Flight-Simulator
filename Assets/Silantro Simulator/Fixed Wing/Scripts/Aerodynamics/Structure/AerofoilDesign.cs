using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oyedoyin;
using Silantro;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif



public class AerofoilDesign
{


    /// <summary>
    /// Does all the "Unclean" stuff for the aerofoils :))
    /// </summary>
#if UNITY_EDITOR
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public static void ShapeAerofoilPlain(SilantroAerofoil foil)
    {
        // ------------------------ Collider Check
        foil.foilCollider = foil.gameObject.GetComponent<BoxCollider>();
        if (foil.foilCollider == null) { foil.foilCollider = foil.gameObject.AddComponent<BoxCollider>(); }
        if (foil.foilCollider != null) { foil.foilCollider.size = new Vector3(1.0f, 0.1f, 1.0f); }


     


        // ------------------------ Extrapolate Panels
        for (int p = 1; p < foil.foilSubdivisions; p++)
        {
            // ----------------- Division Factors
            float currentSection = (float)p, nextSection = (float)(p + 1); float sectionLength = (float)foil.foilSubdivisions; float sectionFactor = currentSection / sectionLength;
            Vector3 LeadingPointA, TrailingPointA;

            // ---------------- Points
            TrailingPointA = MathBase.EstimateSectionPosition(foil.RootChordTrailing, foil.TipChordTrailing, sectionFactor); LeadingPointA = MathBase.EstimateSectionPosition(foil.RootChordLeading, foil.TipChordLeading, sectionFactor);

            // ---------------- Mark
            if (foil.aerofoilType != SilantroAerofoil.AerofoilType.Balance)
            {
                Gizmos.color = Color.yellow; Gizmos.DrawLine(LeadingPointA, TrailingPointA);
                float yM = Vector3.Distance(foil.RootChordTrailing, TrailingPointA);
                if (foil.drawFoils)
                {
                    if (foil.rootAirfoil != null && foil.tipAirfoil != null) { FMath.PlotRibAirfoil(LeadingPointA, TrailingPointA, yM, 0, Color.yellow, foil.drawSplits, foil.rootAirfoil, foil.tipAirfoil, foil.foilSpan, foil.transform); }
                }
            }
        }


        // ------------------------ Wing Mesh
        if (foil.drawMesh)
        {
            if (foil.tipPoints != null && foil.rootPoints != null)
            {
                for (int j = 0; (j < foil.tipPoints.Count / 2); j++) { Vector3 PR = foil.rootPoints[j]; Vector3 PT = foil.tipPoints[j]; Gizmos.color = Color.yellow; Gizmos.DrawLine(PR, PT); }
            }
            else { Debug.LogError("Airfoil for wing " + foil.transform.name + " not plotted properly"); }
        }




        // ------------------------------------- Ground Effect
        if (foil.effectState == SilantroAerofoil.GroundEffectState.Consider)
        {
            for (int p = 1; p < foil.foilSubdivisions; p++)
            {
                float currentSection = (float)p, nextSection = (float)(p + 1); float sectionLength = (float)foil.foilSubdivisions; float sectionFactor = currentSection / sectionLength;
                float sectionPlus = nextSection / sectionLength; Vector3 LeadingPointA, TrailingPointA, LeadingPointB, TrailingPointB;
                TrailingPointA = MathBase.EstimateSectionPosition(foil.RootChordTrailing, foil.TipChordTrailing, sectionFactor); LeadingPointA = MathBase.EstimateSectionPosition(foil.RootChordLeading, foil.TipChordLeading, sectionFactor);
                TrailingPointB = MathBase.EstimateSectionPosition(foil.RootChordTrailing, foil.TipChordTrailing, sectionPlus); LeadingPointB = MathBase.EstimateSectionPosition(foil.RootChordLeading, foil.TipChordLeading, sectionPlus);


                Vector3 geometricCenter = MathBase.EstimateGeometricCenter(LeadingPointA, TrailingPointA, LeadingPointB, TrailingPointB);
                Vector3 baseDirection = foil.transform.rotation * foil.groundAxis;
                Ray groundCheck = new Ray(geometricCenter, baseDirection);
                RaycastHit groundHit;

                if (Physics.Raycast(groundCheck, out groundHit, foil.foilSpan, foil.groundLayer))
                {
                    float spanRatio = groundHit.distance / foil.foilSpan; float blue = spanRatio; float red = spanRatio; spanRatio = Mathf.Clamp(spanRatio, 0.0f, 1.0f); Color lineColor = new Color(red, 1, blue);
                    Debug.DrawLine(geometricCenter, groundHit.point, lineColor);
                    float GA = 1 + (3.142f / (8f * spanRatio));
                    float GB = (2 / (3.142f * 3.142f)) * Mathf.Log(GA);
                    foil.groundInfluenceFactor = 1 - GB;
                }
            }
        }
    }












    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public static void ShapeAerofoil(SilantroAerofoil foil)
    {
        if (foil.debugMode == SilantroAerofoil.DebugMode.Active)
        {
            if (foil.panelLiftForces == null || foil.foilSubdivisions != foil.panelLiftForces.Length)
            {
                foil.panelLiftForces = new float[foil.foilSubdivisions];
                foil.panelDragForces = new float[foil.foilSubdivisions];
                foil.panelMoments = new float[foil.foilSubdivisions];
                foil.panelAOA = new float[foil.foilSubdivisions];
                foil.panelCL = new float[foil.foilSubdivisions];
            }
        }


        // ------------------------ Draw Airfoils
        if (foil.aerofoilType != SilantroAerofoil.AerofoilType.Balance)
        {
            if (foil.drawFoils)
            {
                if (foil.rootAirfoil != null) { FMath.PlotAirfoil(foil.RootChordLeading, foil.RootChordTrailing, foil.rootAirfoil, foil.transform, out foil.rootAirfoilArea, out foil.rootPoints); }
                if (foil.tipAirfoil != null) { FMath.PlotAirfoil(foil.TipChordLeading, foil.TipChordTrailing, foil.tipAirfoil, foil.transform, out foil.tipAirfoilArea, out foil.tipPoints); }
            }
        }



        // ------------------------ Connection Points
        Vector3 adjustedTipCenter = MathBase.EstimateSectionPosition(foil.TipChordLeading, foil.TipChordTrailing, 0.5f);
        Handles.DrawDottedLine(foil.rootChordCenter, adjustedTipCenter, 4f);
        Handles.color = Color.yellow; Handles.DrawDottedLine(foil.quaterRootChordPoint, foil.quaterTipChordPoint, 4f);


        // ------------------------ Connection Points
        Handles.DrawDottedLine(foil.rootChordCenter, adjustedTipCenter, 4f);
        Handles.color = Color.yellow; Handles.DrawDottedLine(foil.quaterRootChordPoint, foil.quaterTipChordPoint, 4f);

        Gizmos.color = Color.yellow; Gizmos.DrawLine(foil.RootChordLeading, foil.TipChordLeading);//LEADING SPAN
        Gizmos.color = Color.red; Gizmos.DrawLine(foil.TipChordTrailing, foil.RootChordTrailing);//TRAILING SPAN
        Handles.color = Color.green; Handles.ArrowHandleCap(0, foil.TipChordLeading, foil.transform.rotation * Quaternion.LookRotation(Vector3.up), 0.3f, EventType.Repaint);




        // ----------------------------------------- Wing Tip
        if (foil.tipDesign == SilantroAerofoil.WingtipDesign.Winglet || foil.tipDesign == SilantroAerofoil.WingtipDesign.Endplate)
        {
            float yMt = Vector3.Distance(foil.RootChordTrailing, foil.wingTipTrailing);
            if (foil.rootAirfoil != null && foil.tipAirfoil != null && foil.drawFoils)
            {
                FMath.PlotRibAirfoil(foil.wingTipLeading, foil.wingTipTrailing, yMt, foil.wingTipHeight, Color.yellow, foil.drawSplits, foil.rootAirfoil, foil.tipAirfoil, foil.foilSpan, foil.transform);
            }
            //DRAW WINGLET OUTLINE
            Gizmos.color = Color.yellow; Gizmos.DrawLine(foil.wingTipLeading, foil.wingTipTrailing);
            Gizmos.color = Color.yellow; Gizmos.DrawLine(foil.wingTipLeading, foil.TipChordLeading);
            Gizmos.color = Color.yellow; Gizmos.DrawLine(foil.wingTipTrailing, foil.TipChordTrailing);
            Gizmos.color = Color.red; Gizmos.DrawLine(foil.wingLetCenter, foil.tipChordCenter);
        }
    }

#endif



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public static void MapControlSurface(SilantroAerofoil foil)
    {
        Color sendColor;

        if (foil.surfaceType == SilantroAerofoil.SurfaceType.Aileron) { foil.controlColor = Color.green; }
        if (foil.surfaceType == SilantroAerofoil.SurfaceType.Elevon) { foil.controlColor = new Color(0, 0, 0.5f); }
        if (foil.surfaceType == SilantroAerofoil.SurfaceType.Elevator) { foil.controlColor = Color.blue; }
        if (foil.surfaceType == SilantroAerofoil.SurfaceType.Rudder) { foil.controlColor = Color.red; }
        if (foil.surfaceType == SilantroAerofoil.SurfaceType.Ruddervator) { foil.controlColor = new Color(0.5f, 0, 0f); }


        // --------------------------------------------------- Base Control
        if (foil.controlSections == null || foil.foilSubdivisions != foil.controlSections.Length)
        {
            foil.controlSections = new bool[foil.foilSubdivisions];
        }
        if (foil.surfaceType != SilantroAerofoil.SurfaceType.Inactive && foil.availableControls != SilantroAerofoil.AvailableControls.SecondaryOnly)
        {
             EstimateControlSurface(foil, foil.controlTipChord, foil.controlRootChord, foil.controlSections, foil.controlColor, false, false, foil.controlDeflection, true, out foil.controlSpan, out foil.controlArea);
        }
        if(foil.availableControls == SilantroAerofoil.AvailableControls.SecondaryOnly) { foil.controlSections = new bool[foil.foilSubdivisions]; }


        // --------------------------------------------------- Flap
        if (foil.flapSections == null || foil.foilSubdivisions != foil.flapSections.Length)
        {
            foil.flapSections = new bool[foil.foilSubdivisions];
        }
        if (foil.flapState == SilantroAerofoil.ControlState.Active && foil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly)
        {
            if (foil.flapType == SilantroAerofoil.FlapType.Flapevon) { sendColor = new Color(0, 0, 0.5f); } else { sendColor = Color.yellow; }
            EstimateControlSurface(foil, foil.flapTipChord, foil.flapRootChord, foil.flapSections, sendColor, false, false, foil.flapDeflection, true, out foil.flapSpan, out foil.flapArea);
        }
        if (foil.availableControls == SilantroAerofoil.AvailableControls.PrimaryOnly) { foil.flapSections = new bool[foil.foilSubdivisions]; }


        // --------------------------------------------------- Slat
        if (foil.slatSections == null || foil.foilSubdivisions != foil.slatSections.Length)
        {
            foil.slatSections = new bool[foil.foilSubdivisions];
        }
        if (foil.slatState == SilantroAerofoil.ControlState.Active && foil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly)
        {
            EstimateControlSurface(foil, foil.slatTipChord, foil.slatRootChord, foil.slatSections, Color.magenta, true, false, 0, true, out foil.slatSpan, out foil.slatArea);
        }
        if (foil.availableControls == SilantroAerofoil.AvailableControls.PrimaryOnly) { foil.slatSections = new bool[foil.foilSubdivisions]; }


        // --------------------------------------------------- Spoilers
        if (foil.spoilerSections == null || foil.foilSubdivisions != foil.spoilerSections.Length)
        {
            foil.spoilerSections = new bool[foil.foilSubdivisions];
        }
        if (foil.spoilerState == SilantroAerofoil.ControlState.Active && foil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly)
        {
            EstimateControlSurface(foil, foil.spoilerChordFactor, foil.spoilerChordFactor, foil.spoilerSections, Color.cyan, false, true, foil.spoilerDeflection, true, out foil.spoilerSpan, out foil.spoilerArea);
        }
        if (foil.availableControls == SilantroAerofoil.AvailableControls.PrimaryOnly) { foil.spoilerSections = new bool[foil.foilSubdivisions]; }
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public static void PlotControlCurves(SilantroAerofoil foil, bool slat)
    {
        // ---------------------------------- CL Base
        foil.clBaseCurve = new AnimationCurve();
        foil.clBaseCurve.AddKey(new Keyframe(6.0144f, 0.9199f));
        foil.clBaseCurve.AddKey(new Keyframe(6.5054f, 0.8883f));
        foil.clBaseCurve.AddKey(new Keyframe(6.8520f, 0.8680f));
        foil.clBaseCurve.AddKey(new Keyframe(7.2274f, 0.8455f));
        foil.clBaseCurve.AddKey(new Keyframe(7.6462f, 0.8252f));
        foil.clBaseCurve.AddKey(new Keyframe(8.0505f, 0.8117f));
        foil.clBaseCurve.AddKey(new Keyframe(8.4693f, 0.8050f));
        foil.clBaseCurve.AddKey(new Keyframe(9.1047f, 0.8051f));
        foil.clBaseCurve.AddKey(new Keyframe(9.8267f, 0.8052f));
        foil.clBaseCurve.AddKey(new Keyframe(10.491f, 0.8280f));
        foil.clBaseCurve.AddKey(new Keyframe(11.083f, 0.8553f));
        foil.clBaseCurve.AddKey(new Keyframe(11.704f, 0.8938f));
        foil.clBaseCurve.AddKey(new Keyframe(12.2238f, 0.9301f));
        foil.clBaseCurve.AddKey(new Keyframe(12.7726f, 0.9619f));
        foil.clBaseCurve.AddKey(new Keyframe(13.2202f, 1.0027f));
        foil.clBaseCurve.AddKey(new Keyframe(13.639f, 1.0458f));
        foil.clBaseCurve.AddKey(new Keyframe(14.1733f, 1.1002f));
        foil.clBaseCurve.AddKey(new Keyframe(14.5921f, 1.1455f));
        foil.clBaseCurve.AddKey(new Keyframe(15.0108f, 1.1954f));
        foil.clBaseCurve.AddKey(new Keyframe(15.4007f, 1.2429f));
        foil.clBaseCurve.AddKey(new Keyframe(15.8339f, 1.2905f));
        foil.clBaseCurve.AddKey(new Keyframe(16.2383f, 1.3404f));
        foil.clBaseCurve.AddKey(new Keyframe(16.657f, 1.3902f));
        foil.clBaseCurve.AddKey(new Keyframe(17.1336f, 1.4446f));
        foil.clBaseCurve.AddKey(new Keyframe(17.5812f, 1.4832f));
        foil.clBaseCurve.AddKey(new Keyframe(17.9134f, 1.5172f));

       
        // ---------------------------------- K1
        foil.k1Curve = new AnimationCurve();
        foil.k1Curve.AddKey(new Keyframe(0f , 0f));
        foil.k1Curve.AddKey(new Keyframe(0.8361f, 0.089f));
        foil.k1Curve.AddKey(new Keyframe(1.5174f, 0.1572f));
        foil.k1Curve.AddKey(new Keyframe(2.1677f, 0.2166f));
        foil.k1Curve.AddKey(new Keyframe(2.9729f, 0.2759f));
        foil.k1Curve.AddKey(new Keyframe(3.9948f, 0.3383f));
        foil.k1Curve.AddKey(new Keyframe(4.9858f, 0.4006f));
        foil.k1Curve.AddKey(new Keyframe(6.0077f, 0.463f));
        foil.k1Curve.AddKey(new Keyframe(7.0606f, 0.5224f));
        foil.k1Curve.AddKey(new Keyframe(8.1445f, 0.5788f));
        foil.k1Curve.AddKey(new Keyframe(9.2903f, 0.6263f));
        foil.k1Curve.AddKey(new Keyframe(10.3432f, 0.665f));
        foil.k1Curve.AddKey(new Keyframe(11.3652f, 0.7007f));
        foil.k1Curve.AddKey(new Keyframe(12.449f, 0.7393f));
        foil.k1Curve.AddKey(new Keyframe(13.471f, 0.7691f));
        foil.k1Curve.AddKey(new Keyframe(14.5239f, 0.8018f));
        foil.k1Curve.AddKey(new Keyframe(15.5148f, 0.8286f));
        foil.k1Curve.AddKey(new Keyframe(16.5058f, 0.8554f));
        foil.k1Curve.AddKey(new Keyframe(17.6206f, 0.8762f));
        foil.k1Curve.AddKey(new Keyframe(18.5187f, 0.8941f));
        foil.k1Curve.AddKey(new Keyframe(19.5097f, 0.915f));
        foil.k1Curve.AddKey(new Keyframe(20.6555f, 0.9359f));
        foil.k1Curve.AddKey(new Keyframe(21.6155f, 0.9597f));
        foil.k1Curve.AddKey(new Keyframe(22.5755f, 0.9746f));
        foil.k1Curve.AddKey(new Keyframe(23.9381f, 0.9955f));

       

        // ---------------------------------- K2
        foil.k2Curve = new AnimationCurve();
        foil.k2Curve.AddKey(new Keyframe(0f, 0f));
        foil.k2Curve.AddKey(new Keyframe(2.2607f, 0.0772f));
        foil.k2Curve.AddKey(new Keyframe(3.6007f, 0.1269f));
        foil.k2Curve.AddKey(new Keyframe(5.2748f, 0.1766f));
        foil.k2Curve.AddKey(new Keyframe(7.2011f, 0.2483f));
        foil.k2Curve.AddKey(new Keyframe(9.5442f, 0.309f));
        foil.k2Curve.AddKey(new Keyframe(11.9714f, 0.3779f));
        foil.k2Curve.AddKey(new Keyframe(14.0639f, 0.4386f));
        foil.k2Curve.AddKey(new Keyframe(16.323f, 0.491f));
        foil.k2Curve.AddKey(new Keyframe(18.6653f, 0.5407f));
        foil.k2Curve.AddKey(new Keyframe(21.0916f, 0.5959f));
        foil.k2Curve.AddKey(new Keyframe(23.8518f, 0.6483f));
        foil.k2Curve.AddKey(new Keyframe(26.4446f, 0.6952f));
        foil.k2Curve.AddKey(new Keyframe(29.0375f, 0.7448f));
        foil.k2Curve.AddKey(new Keyframe(31.7972f, 0.789f));
        foil.k2Curve.AddKey(new Keyframe(34.9739f, 0.8248f));
        foil.k2Curve.AddKey(new Keyframe(37.649f, 0.8552f));
        foil.k2Curve.AddKey(new Keyframe(40.4075f, 0.8828f));
        foil.k2Curve.AddKey(new Keyframe(43.0821f, 0.9048f));
        foil.k2Curve.AddKey(new Keyframe(46.0908f, 0.9269f));
        foil.k2Curve.AddKey(new Keyframe(48.8487f, 0.9462f));
        foil.k2Curve.AddKey(new Keyframe(52.1074f, 0.96f));
        foil.k2Curve.AddKey(new Keyframe(55.1994f, 0.9793f));
        foil.k2Curve.AddKey(new Keyframe(57.7062f, 0.9903f));
        foil.k2Curve.AddKey(new Keyframe(60.0458f, 1.0014f));

      

        // ---------------------------------- K3
        foil.k3Curve = new AnimationCurve();
        foil.k3Curve.AddKey(new Keyframe(0f, 0f));
        foil.k3Curve.AddKey(new Keyframe(0.0445f, 0.0648f));
        foil.k3Curve.AddKey(new Keyframe(0.0915f, 0.1296f));
        foil.k3Curve.AddKey(new Keyframe(0.1288f, 0.1782f));
        foil.k3Curve.AddKey(new Keyframe(0.1613f, 0.2245f));
        foil.k3Curve.AddKey(new Keyframe(0.1974f, 0.2708f));
        foil.k3Curve.AddKey(new Keyframe(0.236f, 0.3148f));
        foil.k3Curve.AddKey(new Keyframe(0.2841f, 0.3727f));
        foil.k3Curve.AddKey(new Keyframe(0.3226f, 0.4167f));
        foil.k3Curve.AddKey(new Keyframe(0.3611f, 0.456f));
        foil.k3Curve.AddKey(new Keyframe(0.3948f, 0.4977f));
        foil.k3Curve.AddKey(new Keyframe(0.4297f, 0.5324f));
        foil.k3Curve.AddKey(new Keyframe(0.4634f, 0.5671f));
        foil.k3Curve.AddKey(new Keyframe(0.4971f, 0.6088f));
        foil.k3Curve.AddKey(new Keyframe(0.5392f, 0.6458f));
        foil.k3Curve.AddKey(new Keyframe(0.591f, 0.6921f));
        foil.k3Curve.AddKey(new Keyframe(0.6319f, 0.7292f));
        foil.k3Curve.AddKey(new Keyframe(0.6824f, 0.7708f));
        foil.k3Curve.AddKey(new Keyframe(0.7257f, 0.8079f));
        foil.k3Curve.AddKey(new Keyframe(0.769f, 0.8472f));
        foil.k3Curve.AddKey(new Keyframe(0.8124f, 0.8773f));
        foil.k3Curve.AddKey(new Keyframe(0.8581f, 0.9097f));
        foil.k3Curve.AddKey(new Keyframe(0.9002f, 0.9375f));
        foil.k3Curve.AddKey(new Keyframe(0.9399f, 0.963f));
        foil.k3Curve.AddKey(new Keyframe(0.9976f, 0.9977f));

        if (slat)
        {
            // ------------------------------------ dCl/dd
            foil.liftDeltaCurve = new AnimationCurve();
            foil.liftDeltaCurve.AddKey(new Keyframe(0.0125f, 0.0063f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.025f, 0.0101f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.0412f, 0.0132f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.0587f, 0.016f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.0774f, 0.0182f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.0961f, 0.0207f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.1173f, 0.0227f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.1398f, 0.0244f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.1672f, 0.026f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.1984f, 0.0279f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.2296f, 0.0294f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.259f, 0.0306f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.2883f, 0.0316f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.3183f, 0.0324f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.3482f, 0.0331f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.3757f, 0.0337f));
            foil.liftDeltaCurve.AddKey(new Keyframe(0.3994f, 0.034f));


            // ------------------------------------- nMax
            foil.nMaxCurve = new AnimationCurve();
            foil.nMaxCurve.AddKey(new Keyframe(0f, 0.675f));
            foil.nMaxCurve.AddKey(new Keyframe(0.0063f, 0.765f));
            foil.nMaxCurve.AddKey(new Keyframe(0.0139f, 0.88f));
            foil.nMaxCurve.AddKey(new Keyframe(0.0215f, 0.985f));
            foil.nMaxCurve.AddKey(new Keyframe(0.0293f, 1.105f));
            foil.nMaxCurve.AddKey(new Keyframe(0.0382f, 1.23f));
            foil.nMaxCurve.AddKey(new Keyframe(0.0454f, 1.34f));
            foil.nMaxCurve.AddKey(new Keyframe(0.053f, 1.45f));
            foil.nMaxCurve.AddKey(new Keyframe(0.0618f, 1.565f));
            foil.nMaxCurve.AddKey(new Keyframe(0.07f, 1.65f));
            foil.nMaxCurve.AddKey(new Keyframe(0.0795f, 1.7f));
            foil.nMaxCurve.AddKey(new Keyframe(0.0902f, 1.705f));
            foil.nMaxCurve.AddKey(new Keyframe(0.1022f, 1.675f));
            foil.nMaxCurve.AddKey(new Keyframe(0.1158f, 1.595f));
            foil.nMaxCurve.AddKey(new Keyframe(0.1271f, 1.485f));
            foil.nMaxCurve.AddKey(new Keyframe(0.1394f, 1.375f));
            foil.nMaxCurve.AddKey(new Keyframe(0.1502f, 1.275f));
            foil.nMaxCurve.AddKey(new Keyframe(0.1603f, 1.18f));
            foil.nMaxCurve.AddKey(new Keyframe(0.1691f, 1.1f));
            foil.nMaxCurve.AddKey(new Keyframe(0.1804f, 1f));
            foil.nMaxCurve.AddKey(new Keyframe(0.2003f, 0.815f));


            // -------------------------------------- nDelta
            foil.nDeltaCurve = new AnimationCurve();
            foil.nDeltaCurve.AddKey(new Keyframe(-0.0482f, 0.9988f));
            foil.nDeltaCurve.AddKey(new Keyframe(5.0513f, 0.9953f));
            foil.nDeltaCurve.AddKey(new Keyframe(9.9183f, 1.0059f));
            foil.nDeltaCurve.AddKey(new Keyframe(14.844f, 1.0024f));
            foil.nDeltaCurve.AddKey(new Keyframe(19.0775f, 0.9318f));
            foil.nDeltaCurve.AddKey(new Keyframe(22.3273f, 0.8329f));
            foil.nDeltaCurve.AddKey(new Keyframe(25.6939f, 0.7165f));
            foil.nDeltaCurve.AddKey(new Keyframe(29.4085f, 0.5929f));
            foil.nDeltaCurve.AddKey(new Keyframe(32.0784f, 0.5012f));
            foil.nDeltaCurve.AddKey(new Keyframe(34.8647f, 0.4024f));
        }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public static void EstimateControlSurface(SilantroAerofoil foil, float inputTipChord, float inputRootChord, bool[] sections, Color surfaceColor, bool leading, bool floating, float surfaceDeflection, bool drawSections, out float surfaceSpan, out float surfaceArea)
    {
        //COLLECTION
        surfaceArea = 0; surfaceSpan = 0f;

        if (sections != null)
        {
            for (int i = 0; i < sections.Length; i++)
            {
                if (sections[i] == true)
                {
                    //BUILD VARIABLES
                    float currentSelection = (float)i; float nextSection = (float)(i + 1); float sectionLength = (float)sections.Length;
                    float baseFactorA = currentSelection / sectionLength; float baseFactorB = nextSection / sectionLength;
                    //DRAW CONTROL SURFACE
                    Vector3[] rects = new Vector3[4];
                    if (floating)
                    {
                        rects[0] = MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(foil.RootChordLeading, foil.RootChordTrailing, (foil.spoilerHinge * 0.01f)),
                            MathBase.EstimateSectionPosition(foil.TipChordLeading, foil.TipChordTrailing, (foil.spoilerHinge * 0.01f)), baseFactorA);
                        rects[1] = MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(foil.RootChordLeading, foil.RootChordTrailing, (foil.spoilerHinge * 0.01f)), foil.RootChordTrailing, (inputRootChord * 0.01f)),
                            MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(foil.TipChordLeading, foil.TipChordTrailing, (foil.spoilerHinge * 0.01f)), foil.TipChordTrailing, (inputTipChord * 0.01f)), baseFactorA);
                        rects[2] = MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(foil.RootChordLeading, foil.RootChordTrailing, (foil.spoilerHinge * 0.01f)), foil.RootChordTrailing, (inputRootChord * 0.01f)),
                            MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(foil.TipChordLeading, foil.TipChordTrailing, (foil.spoilerHinge * 0.01f)), foil.TipChordTrailing, (inputTipChord * 0.01f)), baseFactorB);
                        rects[3] = MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(foil.RootChordLeading, foil.RootChordTrailing, (foil.spoilerHinge * 0.01f)),
                            MathBase.EstimateSectionPosition(foil.TipChordLeading, foil.TipChordTrailing, (foil.spoilerHinge * 0.01f)), baseFactorB);
                    }
                    else
                    {
                        if (!leading)
                        {
                            //TRAILING  EDGE CONTROLS
                            rects[0] = (MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(foil.RootChordTrailing, foil.RootChordLeading, (inputRootChord * 0.01f)),
                                MathBase.EstimateSectionPosition(foil.TipChordTrailing, foil.TipChordLeading, (inputTipChord * 0.01f)), baseFactorA));
                            rects[1] = (MathBase.EstimateSectionPosition(foil.RootChordTrailing, foil.TipChordTrailing, baseFactorA));
                            rects[2] = (MathBase.EstimateSectionPosition(foil.RootChordTrailing, foil.TipChordTrailing, baseFactorB));
                            rects[3] = (MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(foil.RootChordTrailing, foil.RootChordLeading, (inputRootChord * 0.01f)),
                                MathBase.EstimateSectionPosition(foil.TipChordTrailing, foil.TipChordLeading, (inputTipChord * 0.01f)), baseFactorB));
                        }
                        else
                        {
                            //LEADING EDGE CONTROLS
                            rects[0] = MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(foil.RootChordLeading, foil.RootChordTrailing, (inputRootChord * 0.01f)),
                                MathBase.EstimateSectionPosition(foil.TipChordLeading, foil.TipChordTrailing, (inputTipChord * 0.01f)), baseFactorA);
                            rects[1] = (MathBase.EstimateSectionPosition(foil.RootChordLeading, foil.TipChordLeading, baseFactorA));
                            rects[2] = (MathBase.EstimateSectionPosition(foil.RootChordLeading, foil.TipChordLeading, baseFactorB));
                            rects[3] = MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(foil.RootChordLeading, foil.RootChordTrailing, (inputRootChord * 0.01f)),
                                MathBase.EstimateSectionPosition(foil.TipChordLeading, foil.TipChordTrailing, (inputTipChord * 0.01f)), baseFactorB);
                        }
                    }

                    //DEFLECT SURFACE
                    rects[1] = rects[0] + Quaternion.AngleAxis(surfaceDeflection, (rects[3] - rects[0]).normalized) * (rects[1] - rects[0]);
                    rects[2] = rects[3] + (Quaternion.AngleAxis(surfaceDeflection, (rects[3] - rects[0]).normalized)) * (rects[2] - rects[3]);

#if UNITY_EDITOR
                    //DRAW CONTROL AIRFOILS
                    if (drawSections && (foil.rootAirfoil != null && foil.tipAirfoil != null))
                    {
                        float yM = Vector3.Distance(foil.RootChordTrailing, rects[1]);
                        if (foil.drawFoils) { if ((foil.rootAirfoil != null && foil.tipAirfoil != null)) { FMath.PlotRibAirfoil(rects[0], rects[1], yM, 0, Color.white, foil.drawSplits, foil.rootAirfoil, foil.tipAirfoil, foil.foilSpan, foil.transform); } }
                    }
                    //DRAW CONTROLS
                    Handles.color = surfaceColor;
                    Handles.DrawSolidRectangleWithOutline(rects, surfaceColor, surfaceColor);

#endif
                    //ANALYSIS
                    surfaceArea += MathBase.EstimatePanelSectionArea(rects[0], rects[3], rects[1], rects[2]);
                    Vector3 rootCenter = MathBase.EstimateSectionPosition(rects[0], rects[1], 0.5f); Vector3 tipCenter = MathBase.EstimateSectionPosition(rects[2], rects[3], 0.5f);
                    float panelSpan = Vector3.Distance(rootCenter, tipCenter);
                    float Cr = Vector3.Distance(rects[0], rects[1]); float Ct = Vector3.Distance(rects[2], rects[3]); surfaceSpan += panelSpan;
                    if (foil.spoilerSections[i] == true) { foil.spoilerDragFactor = foil.spoilerDragCurve.Evaluate(panelSpan / ((Cr + Ct) / 2)); }
                }
            }
        }
    }








    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public static void MapWingPlate(SilantroAerofoil foil)
    {
        //WINGLET
        Vector3 wingletDirection = foil.transform.up.normalized; float actualSweep = foil.wingTipSweep; if (foil.transform.localScale.x < 0) { actualSweep *= -1; }
        foil.wingLetCenter = (MathBase.EstimateSectionPosition(foil.TipChordLeading, foil.TipChordTrailing, 0.5f) + (foil.transform.right * (foil.wingTipHeight * 0.5f))) + (foil.transform.forward * (actualSweep / 90) * 1);

        Vector3 tipDirection = foil.wingLetCenter - foil.tipChordCenter;
        float actualBend = foil.wingTipBend; if (foil.transform.localScale.x < 0) { tipDirection *= -1; actualBend *= -1; }
        tipDirection = Quaternion.Euler(foil.transform.forward.normalized * actualBend) * tipDirection;
        foil.wingLetCenter = tipDirection + foil.tipChordCenter;


        Vector3 tipFactor = foil.transform.forward * ((foil.foilTipChord * 0.5f) * (foil.TipTaperPercentage / 100));
        foil.wingTipLeading = (foil.wingLetCenter + tipFactor);
        foil.wingTipTrailing = foil.wingLetCenter - tipFactor;
        foil.wingTipPivot = (Quaternion.AngleAxis(foil.trueTwist, foil.transform.rotation * foil.transform.right.normalized) * (foil.wingTipLeading - foil.wingTipTrailing)) * 0.5f;
        foil.wingTipLeading = foil.wingLetCenter + foil.wingTipPivot; foil.wingTipTrailing = foil.wingLetCenter - foil.wingTipPivot;
    }









    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public static void MapEfficiency(SilantroAerofoil foil)
    {
        // --------------------------------- Base Surface
        if (foil.controlArea > 0 && foil.surfaceType != SilantroAerofoil.SurfaceType.Inactive && foil.availableControls != SilantroAerofoil.AvailableControls.SecondaryOnly)
        {
            {
                foil.controlNullPoint = MathBase.EstimateEfficiencySpeed(0.01f, foil.controlArea, foil.maximumControlTorque);
                foil.controlLockPoint = MathBase.EstimateEfficiencySpeed(0.1f, foil.controlArea, foil.maximumControlTorque);
                foil.controlFullPoint = MathBase.EstimateEfficiencySpeed(1, foil.controlArea, foil.maximumControlTorque);

                foil.controlEfficiencyCurve = new AnimationCurve();
                for (float i = foil.controlFullPoint; i < 1000f; i += 50f)
                {
                    float efficiency = MathBase.EstimateControlEfficiency(i, foil.controlArea, foil.maximumControlTorque);
                    foil.controlEfficiencyCurve.AddKey(new Keyframe(i * 1.944f, efficiency * 100f));
                }
            }
        }




        // --------------------------------- Flap Surface
        if (foil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly)
        {
            if (foil.flapState == SilantroAerofoil.ControlState.Active)
            {
                if (foil.flapArea > 0)
                {
                    foil.flapNullPoint = MathBase.EstimateEfficiencySpeed(0.01f, foil.flapArea, foil.maximumFlapTorque);
                    foil.flapLockPoint = MathBase.EstimateEfficiencySpeed(0.1f, foil.flapArea, foil.maximumFlapTorque);
                    foil.flapFullPoint = MathBase.EstimateEfficiencySpeed(1, foil.flapArea, foil.maximumFlapTorque);

                    foil.flapEfficiencyCurve = new AnimationCurve();
                    for (float i = foil.flapFullPoint; i < 1000f; i += 50f)
                    {
                        float efficiency = MathBase.EstimateControlEfficiency(i, foil.flapArea, foil.maximumFlapTorque);
                        foil.flapEfficiencyCurve.AddKey(new Keyframe(i * 1.944f, efficiency * 100f));
                    }
                }
            }


            // --------------------------------- Spoiler Surface
            if (foil.spoilerState == SilantroAerofoil.ControlState.Active)
            {
                if (foil.spoilerArea > 0)
                {
                    foil.spoilerNullPoint = MathBase.EstimateEfficiencySpeed(0.01f, foil.spoilerArea, foil.maximumSpoilerTorque);
                    foil.spoilerLockPoint = MathBase.EstimateEfficiencySpeed(0.1f, foil.spoilerArea, foil.maximumSpoilerTorque);
                    foil.spoilerFullPoint = MathBase.EstimateEfficiencySpeed(1, foil.spoilerArea, foil.maximumSpoilerTorque);

                    foil.spoilerEfficiencyCurve = new AnimationCurve();
                    for (float i = foil.spoilerFullPoint; i < 1000f; i += 50f)
                    {
                        float efficiency = MathBase.EstimateControlEfficiency(i, foil.spoilerArea, foil.maximumSpoilerTorque);
                        foil.spoilerEfficiencyCurve.AddKey(new Keyframe(i * 1.944f, efficiency * 100f));
                    }
                }
            }
        }
    }
}

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Oyedoyin;
using Silantro;



/// <summary>
///
/// 
/// Use:		 Handles the calculation/analysis of aerodynamic force and moments on the aircraft
/// </summary>




[RequireComponent(typeof(BoxCollider))]
public class SilantroAerofoil : MonoBehaviour
{

    // ------------------------------ Selectibles
    public enum AerofoilType { Wing, Stabilizer, Canard, Balance, Stabilator }
    [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")] public AerofoilType aerofoilType = AerofoilType.Wing;
    public enum WingType { Conventional, Delta }
    public WingType wingType = WingType.Conventional;
    public enum SurfaceFinish { SmoothPaint, PolishedMetal, ProductionSheetMetal, MoldedComposite, PaintedAluminium }
    public SurfaceFinish surfaceFinish = SurfaceFinish.PaintedAluminium;
    public enum SweepDirection { Unswept, Forward, Backward }
    public SweepDirection sweepDirection = SweepDirection.Unswept;
    public enum WingtipDesign { Round, Spherical, Square, Endplate, Winglet }
    public WingtipDesign tipDesign = WingtipDesign.Square;
    public enum SweepCorrection { RaymerJenkinson, YoungLE, None }
    public SweepCorrection sweepCorrectionMethod = SweepCorrection.YoungLE;
    public enum GroundEffectMethod { Weiselburger, McCormick, Asselin }
    public GroundEffectMethod groundInfluenceMethod = GroundEffectMethod.Weiselburger;
    public enum TwistDirection { Untwisted, Upwards, Downwards }
    public TwistDirection twistDirection = TwistDirection.Untwisted;
    public enum StabilizerType { Static, Trimmable }
    public StabilizerType stabilizerType = StabilizerType.Static;
    public enum StabilizerOrientation { Vertical, Horizontal }
    public StabilizerOrientation stabOrientation = StabilizerOrientation.Horizontal;
    public enum StabilizerPosition { Left, Right, Center }
    public StabilizerPosition stabilizerPosition = StabilizerPosition.Center;
    public enum WingAlignment { Top, Bottom, Monoplane }
    public WingAlignment wingAlignment = WingAlignment.Monoplane;
    public enum WingPosition { Left, Right, Center }
    public WingPosition wingPosition = WingPosition.Center;
    public enum ControlType { Stationary, Controllable }
    public ControlType controlState = ControlType.Stationary;
    public enum AvailableControls { PrimaryOnly, PrimaryPlusSecondary, SecondaryOnly }
    public AvailableControls availableControls = AvailableControls.PrimaryOnly;
    public enum SlatMovement { Deflection, Extension }
    public SlatMovement slatMovement = SlatMovement.Deflection;
    public enum CanardPosition { Left, Right }
    public CanardPosition canardPosition = CanardPosition.Left;
    public enum RudderPosition { Left, Center, Right }
    public RudderPosition rudderPosition = RudderPosition.Center;
    public enum SpoilerType { Plain, Spoileron }
    public SpoilerType spoilerType = SpoilerType.Plain;
    public enum SurfaceType { Inactive, Elevator, Rudder, Aileron, Ruddervator, Elevon }
    public SurfaceType surfaceType = SurfaceType.Aileron;
    public enum FlapType { Plain, Split, Flaperon, Flapevon }
    public FlapType flapType = FlapType.Plain;
    public enum DebugMode { Off, Active }
    public DebugMode debugMode = DebugMode.Off;
    public enum LiftMethod { Linear, Corrected }
    public LiftMethod liftMethod = LiftMethod.Corrected;
    public enum FlapAngleSetting { ThreeStep, FiveStep }
    public FlapAngleSetting flapAngleSetting = FlapAngleSetting.ThreeStep;
    public enum VortexLift { Consider, Neglect }
    public VortexLift vortexState = VortexLift.Neglect;
    public enum StabilatorType { Plain, Coupled }
    public StabilatorType stabilatorType = StabilatorType.Plain;
    public enum GroundEffectState { Neglect, Consider }
    public GroundEffectState effectState = GroundEffectState.Neglect;
    public enum DeflectionType { Symmetric, Asymmetric }
    public DeflectionType deflectionType = DeflectionType.Symmetric;
    public enum TrimState { Absent, Available }
    public TrimState trimState = TrimState.Absent;
    public enum FlapPosition { Left, Right }
    public FlapPosition flapPosition = FlapPosition.Left;
    public enum AnalysisMethod { GeometricOnly, NumericOnly, Combined }
    public AnalysisMethod flapAnalysis = AnalysisMethod.GeometricOnly;
    public AnalysisMethod controlAnalysis = AnalysisMethod.GeometricOnly;
    public enum NumericCorrection { DATCOM, KhanNahon}
    public NumericCorrection controlCorrectionMethod = NumericCorrection.DATCOM;
    public NumericCorrection flapCorrectionMethod = NumericCorrection.DATCOM;
    public enum ModelType { Internal, Actuator, None }
    public ModelType controlModelType = ModelType.Internal;
    public ModelType flapModelType = ModelType.Internal;
    public ModelType slatModelType = ModelType.Internal;
    public ModelType spoilerModelType = ModelType.Internal;
    public enum SurfaceModelType { Single, Dual }
    public SurfaceModelType baseSurfaceType = SurfaceModelType.Single;
    public SurfaceModelType flapSurfaceType = SurfaceModelType.Single;
    public SurfaceModelType slatSurfaceType = SurfaceModelType.Single;
    public SurfaceModelType spoilerSurfaceType = SurfaceModelType.Single;
    public enum AileronCouple { Absent, Available}
    public AileronCouple aileronCouple = AileronCouple.Absent;
    public float baseAileron;


    // ------------------------------ Connections
    public SilantroController connectedAircraft;
    public SilantroCore coreSystem;
    public SilantroAirfoil rootAirfoil, tipAirfoil;
    public BoxCollider foilCollider;
    private SilantroAerofoil foil;



    // ------------------------------ Shape Dimensions
    public float foilLength, foilWidth, quaterSweep, tipCenterSweep;
    public float sweepAngle, aspectRatio, foilSpan, foilRootChord, foilTipChord, leadingEdgeSweep;
    public float foilArea, foilMeanChord, foilWettedArea;
    [HideInInspector] public Vector3 rootChordCenter, tipChordCenter, quaterRootChordPoint, quaterTipChordPoint;
    public Vector3 RootChordLeading, TipChordLeading, RootChordTrailing, TipChordTrailing, baseTip;
    public Vector3 baseTipLeading, baseTipTrailing;
    public float rootAirfoilArea, tipAirfoilArea;
    public List<Vector3> tipPoints, rootPoints;
    public float stabilizerDeflection, positiveStabLimit = 10f, negativeStabLimit = 10f;
    public float baseFlaperonDeflection, baseFlapevonDeflection, baseFlapevonInput, maxFlap;


    // ------------------------------ Variables
    [Range(3, 15)] public int foilSubdivisions = 5;
    [Range(0, 90f)] public float aerofoilSweepAngle;
    [Range(0, 95f)] public float taperPercentage;
    public float foilDeflection;
    [Range(0, 90f)] public float foilTwist;
    public float foilTipLeadingExtension, foilTipTrailingExtension;
    public float spanEfficiency = 0.9f;
    public float sweepCorrectionFactor = 1f, effectiveChange;
    public Color controlColor = Color.green;
    public float taperRatio, trueTwist;
    public float slatLiftCoefficient, slatDragCoefficient;
    public float flapLiftCoefficient, flapDragCoefficient;
    public float controlLiftCoefficient, controlDragCoefficient;


    // ------------------------------ Control Bools
    public bool drawFoils = true;
    public bool drawSplits;
    public bool drawMesh;
    public bool slatExtended;
    public bool spoilerExtended;
    public bool airbrakeActive;


    // ------------------------------ Control
    [Range(0, 100f)] public float controlRootChord = 10f, flapRootChord = 10f, slatRootChord = 10f;
    [Range(0, 100f)] public float controlTipChord = 10f, flapTipChord = 10f, slatTipChord = 10f, spoilerChordFactor = 20f;
    public float controlArea, flapArea, spoilerArea, slatArea;
    public float controlSpan, flapSpan, spoilerSpan, slatSpan;
    public float maximumSlatDeflection = 25f;
    public float rootTrailingFactor, rootLeadingFactor, rootExtension, tipExtension;
    public float correctedDeflection, Asf, k;
    public bool[] controlSections, flapSections, slatSections, spoilerSections;
    [Range(0, 100f)] public float spoilerHinge = 10f;
    public float spoilerDragFactor;
    [Range(0, 90f)] public float spoilerRollCoupling = 60f;
    public float flapDeflection, takeOffFlap = 20f, flapLimit = 60f, baseFlap, baseSlat, baseSpoiler;
    public List<float> flapAngles; public int flapSetting;
    public float vortexLiftPercentage, vortexDragPercentage, baseFlapDeflection, baseSpoilerDeflection, baseSpoileronDeflection;
    [Range(10, 95f)] public float stabilatorCouplingPercentage = 30f;
    public float posLimit, negLimit = 0;


    // ------------------------------- Ground Effect
    public Vector3 groundAxis;
    public LayerMask groundLayer;
    public float groundInfluenceFactor;



    // ------------------------------ Winglet
    public Vector3 wingLetCenter, wingTipLeading, wingTipTrailing, wingTipPivot;



    // -------------------------------- Sound
    AudioSource clampSource, loopSource;
    public AudioClip flapLoop, flapClamp;
    public AudioClip slatLoop, slatClamp;
    public bool deflectionEngaged;
    public int flapMode;



    // ------------------------------ Actuator
    public float controlActuationSpeed = 60f, flapActuationSpeed = 30f, spoilerActuationSpeed = 50, slatActuationSpeed = 40f;
    public float maximumControlTorque = 5000f, maximumSpoilerTorque = 5000f, maximumFlapTorque = 7000f;
    public float controlActuatorDeflection, flapActuatorDeflection, spoilerActuatorDeflection, baseDeflection, slatPosition, slatDeflection;
    public AnimationCurve controlEfficiencyCurve, flapEfficiencyCurve, spoilerEfficiencyCurve, spoilerDragCurve;
    public float currentControlEfficiency, currentFlapEfficiency, currentSpoilerEfficiency;
    public float positiveLimit = 30f, negativeLimit = 20f, controlDeflection, spoilerDeflection;
    public float positiveFlaperonLimit = 25f, negativeFlaperonLimit = 20f;
    public float positiveTrimLimit = 5f, negativeTrimLimit = 3f, trimInput, trimDeflection;
    public float flapA1 = 10f, flapA2 = 30f, maximumSpoilerDeflection = 60f;

    public float controlLockPoint, controlNullPoint, controlFullPoint;
    public float flapLockPoint, flapNullPoint, flapFullPoint;
    public float spoilerLockPoint, spoilerNullPoint, spoilerFullPoint;
    public float tipSweep, spoilerFactor;


    // ------------------------------ End Factors
    public float TipTaperPercentage = 100f;
    public float wingTipHeight = 5f;
    [Range(-90, 90)] public float wingTipBend = 20f;
    public float wingTipSweep = 20f;
    public float biplaneLiftFactor = 1f, biplaneDragFactor = 1f;

    public List<string> toolbarStrings;
    public int toolbarTab;
    public string currentTab;


    public enum RotationAxis { X, Y, Z }
    public RotationAxis controlDeflectionAxis, dualControlDeflectionAxis = RotationAxis.X;
    public RotationAxis flapDeflectionAxis, dualFlapDeflectionAxis = RotationAxis.X;
    public RotationAxis slatActuationAxis, dualSlatActuationAxis = RotationAxis.X;
    public RotationAxis spoilerDeflectionAxis, dualSpoilerDeflectionAxis = RotationAxis.X;
    public RotationAxis stabilizerDeflectionAxis = RotationAxis.X;

    public enum DeflectionDirection { CW, CCW }
    public DeflectionDirection controlDeflectionDirection, dualControlDirection = DeflectionDirection.CW;
    public DeflectionDirection flapDeflectionDirection, dualFlapDirection = DeflectionDirection.CW;
    public DeflectionDirection slatDeflectionDirection, dualSlatDirection = DeflectionDirection.CW;
    public DeflectionDirection spoilerDeflectionDirection, dualSpoilerDirection = DeflectionDirection.CW;
    public DeflectionDirection stabilizerDeflectionDirection = DeflectionDirection.CW;

    // ------------------------------ Surface Movement
    public Transform controlSurfaceModel, flapSurfaceModel, slatSurfaceModel, spoilerSurfaceModel, stabilizerModel;
    public Transform dualControlSurfaceModel, dualFlapSurfaceModel, dualSlatSurfaceModel, dualSpoilerSurfaceModel;



    public Vector3 controlAxis, dualControlAxis; Quaternion baseControlRotation, dualControlRotation;
    public Vector3 flapAxis, dualFlapAxis; Quaternion baseFlapRotation, dualFlapRotation;
    public Vector3 slatAxis, dualSlatAxis, baseSlatPosition, dualSlatPosition; Quaternion baseSlatRotation, dualSlatRotation;
    public Vector3 spoilerAxis, dualSpoilerAxis; Quaternion baseSpoilerRotation, dualSpoilerRotation;
    public Vector3 stabilizerAxis; Quaternion baseStabilizerRotation;


    public enum ControlState { Absent, Active }
    public ControlState flapState = ControlState.Absent;
    public ControlState slatState = ControlState.Absent;
    public ControlState spoilerState = ControlState.Absent;

    public AnimationCurve clBaseCurve;
    public AnimationCurve k1Curve, k2Curve, k3Curve;
    public AnimationCurve liftDeltaCurve, nMaxCurve, nDeltaCurve;
    public AnimationCurve effectivenessPlot;

    public SilantroActuator controlActuator, flapActuator, slatActuator, spoilerActuator;
    public float controlLevel, flapLevel, slatLevel, spoilerLevel;

    public float rootAlpha = 0f, tipAlpha = 0f;
    public float baseLiftCoefficient, baseDragCoefficient;
    public float panelDragCoefficient, panelMomentCoefficient, panelLiftCoefficient;
    public float flapCoefficient, controlCoefficient, panelFlapLiftCoefficient, panelFlapDragCoefficient;


    // ----------------------------- Inputs
    public float pitchInput, rollInput, yawInput, stabilizerInput;
    public float pitchTrimInput, rollTrimInput, yawTrimInput;
    private float baseInput;




    // ----------------------------- Debug
    public float[] panelLiftForces;
    public float[] panelDragForces;
    public float[] panelMoments;
    public float[] panelAOA;
    public float[] panelCL;


    // ------------------------------ Output
    public float maximumAOA, stallAOA;
    public float TotalLift, TotalDrag, TotalMoment, TotalSkinDrag;
    public float TotalSpoilerDrag, TotalBaseDrag, TotalFlapDrag, TotalControlDrag;


    // ------------------------------ Optimization
    public string twistString;
    public string sweepString;
    public float aspectRatioCorrection;










    // ---------------------------------------------------CONTROLS-------------------------------------------------------------------------------------------------------

    public void SetFlaps(int flapSettingPoint, int mode)
    {
        if (aerofoilType == AerofoilType.Wing && availableControls != AvailableControls.PrimaryOnly && flapState == ControlState.Active && flapType != FlapType.Flapevon)
        {   
            if (loopSource != null && loopSource.isPlaying) { loopSource.Stop(); }
            if (clampSource != null && clampSource.isPlaying) { clampSource.Stop(); }
            flapSetting = flapSettingPoint;
            deflectionEngaged = true; flapMode = mode;
        }
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void RaiseFlap()
    {
        flapSetting -= 1; if (flapSetting < 0) { flapSetting = 0; }
        if (loopSource != null && loopSource.isPlaying) { loopSource.Stop(); }
        if (clampSource != null && clampSource.isPlaying) { clampSource.Stop(); }
        deflectionEngaged = true; flapMode = 1;
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void LowerFlap()
    {
        flapSetting += 1; if (flapSetting > (flapAngles.Count - 1)) { flapSetting = flapAngles.Count - 1; }
        if (loopSource != null && loopSource.isPlaying) { loopSource.Stop(); }
        if (clampSource != null && clampSource.isPlaying) { clampSource.Stop(); }
        deflectionEngaged = true; flapMode = 2;
    }




   
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ActuateSlat() { if (!slatExtended){baseSlat = maximumSlatDeflection;  StartCoroutine(ExtendSlat());} else { baseSlat = 0f;  StartCoroutine(RetractSlat()); } }
    public IEnumerator ExtendSlat()  { yield return new WaitUntil(() => slatDeflection >= maximumSlatDeflection - 1f); slatExtended = true; }
    public IEnumerator RetractSlat() { yield return new WaitUntil(() => slatDeflection <= 1f);  slatExtended = false; }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ActuateSpoiler() { if (!spoilerExtended) { baseSpoiler = maximumSpoilerDeflection; StartCoroutine(ExtendSpoiler()); } else { baseSpoiler = 0f; StartCoroutine(RetractSpoiler()); } }
    public IEnumerator ExtendSpoiler() { yield return new WaitUntil(() => Mathf.Abs(spoilerDeflection) >= maximumSpoilerDeflection - 1f); spoilerExtended = true; }
    public IEnumerator RetractSpoiler() { yield return new WaitUntil(() => Mathf.Abs(spoilerDeflection) <= 1f); spoilerExtended = false; }












    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    bool allOk;
    protected void _checkPrerequisites()
    {

        //---------------------------- Check Components

        if (connectedAircraft != null && coreSystem != null && rootAirfoil != null && tipAirfoil != null)
        {
            allOk = true;
        }
        else if (connectedAircraft == null)
        {
            Debug.LogError("Prerequisites not met on " + aerofoilType.ToString() + " " + transform.name + "....Aircraft controller not assigned");
            allOk = false;
        }
        else if (coreSystem == null)
        {
            Debug.LogError("Prerequisites not met on " + aerofoilType.ToString() + " " + transform.name + "....Core system not assigned");
            allOk = false;
        }
        else if (rootAirfoil == null)
        {
            Debug.LogError("Prerequisites not met on " + aerofoilType.ToString() + " " + transform.name + "....Root airfoil has not been assigned");
            allOk = false;
        }
        else if (tipAirfoil == null)
        {
            Debug.LogError("Prerequisites not met on " + aerofoilType.ToString() + " " + transform.name + "....Tip airfoil has not been assigned");
            allOk = false;
        }
    }








    public void InitializeFoil()
    {

        // ----------------------------
        _checkPrerequisites();



        if (allOk)
        {
            stallAOA = 90f;
            // ------------------------ Collect Foil
            if (foil == null) { foil = this.GetComponent<SilantroAerofoil>(); }
            if (flapAngleSetting == FlapAngleSetting.ThreeStep) { flapAngles.Add(0); flapAngles.Add(takeOffFlap); flapAngles.Add(flapLimit); }
            else { flapAngles.Add(0); flapAngles.Add(flapA1); flapAngles.Add(takeOffFlap); flapAngles.Add(flapA2); flapAngles.Add(flapLimit); }
            sweepString = sweepDirection.ToString(); twistString = twistDirection.ToString();
            effectivenessPlot = FMath.PlotControlEffectiveness();

            //---------------Comment out to Debug
            debugMode = DebugMode.Off;

          

            if (rootAirfoil != null && tipAirfoil != null)
            {
                if (rootAirfoil.stallAngle < stallAOA) { stallAOA = rootAirfoil.stallAngle; }
                if (tipAirfoil.stallAngle < stallAOA) { stallAOA = tipAirfoil.stallAngle; }
                if (stallAOA == 0 || stallAOA > 90) { stallAOA = 15f; }
            }


            // ------------------------------ Calculate Dimensions
            AnalyseDimensions(); if (aerofoilType != AerofoilType.Balance && controlState == ControlType.Controllable) { AerofoilDesign.PlotControlCurves(foil, aerofoilType == AerofoilType.Wing && slatState == ControlState.Active); }


            if (controlState == ControlType.Controllable)
            {
                // ------------------------------ Store Base Model Rotations
                if (controlSurfaceModel != null) { baseControlRotation = controlSurfaceModel.transform.localRotation; }
                if (flapSurfaceModel != null && flapState == ControlState.Active) { baseFlapRotation = flapSurfaceModel.transform.localRotation; }
                if (slatSurfaceModel != null && slatState == ControlState.Active) { baseSlatRotation = slatSurfaceModel.transform.localRotation; baseSlatPosition = slatSurfaceModel.localPosition; }
                if (spoilerSurfaceModel != null && spoilerState == ControlState.Active) { baseSpoilerRotation = spoilerSurfaceModel.transform.localRotation; }
                if (stabilizerModel != null && stabilizerType == StabilizerType.Trimmable) { baseStabilizerRotation = stabilizerModel.transform.localRotation; }

                // ------------------------------ Store Dual Model Rotations
                if (dualControlSurfaceModel != null) { dualControlRotation = dualControlSurfaceModel.transform.localRotation; }
                if (dualFlapSurfaceModel != null && flapState == ControlState.Active) { dualFlapRotation = dualFlapSurfaceModel.transform.localRotation; }
                if (dualSlatSurfaceModel != null && slatState == ControlState.Active) { dualSlatRotation = dualSlatSurfaceModel.transform.localRotation; dualSlatPosition = dualSlatSurfaceModel.localPosition; }
                if (dualSpoilerSurfaceModel != null && spoilerState == ControlState.Active) { dualSpoilerRotation = dualSpoilerSurfaceModel.transform.localRotation; }


                // -----------------------------Base Deflection Axis
                controlAxis = Handler.EstimateModelProperties(controlDeflectionDirection.ToString(), controlDeflectionAxis.ToString()); dualControlAxis = Handler.EstimateModelProperties(dualControlDirection.ToString(), dualControlDeflectionAxis.ToString());
                flapAxis = Handler.EstimateModelProperties(flapDeflectionDirection.ToString(), flapDeflectionAxis.ToString()); dualFlapAxis = Handler.EstimateModelProperties(dualFlapDirection.ToString(), dualFlapDeflectionAxis.ToString());
                slatAxis = Handler.EstimateModelProperties(slatDeflectionDirection.ToString(), slatActuationAxis.ToString()); dualSlatAxis = Handler.EstimateModelProperties(dualSlatDirection.ToString(), dualSlatActuationAxis.ToString());
                spoilerAxis = Handler.EstimateModelProperties(spoilerDeflectionDirection.ToString(), spoilerDeflectionAxis.ToString()); dualSpoilerAxis = Handler.EstimateModelProperties(dualSpoilerDirection.ToString(), dualSpoilerDeflectionAxis.ToString());
                stabilizerAxis = Handler.EstimateModelProperties(stabilizerDeflectionDirection.ToString(), stabilizerDeflectionAxis.ToString());


                // ----------------------------- Setup Sound
                if (controlState == ControlType.Controllable && aerofoilType == AerofoilType.Wing)
                {
                    if (flapState == ControlState.Active || slatState == ControlState.Active)
                    {
                        if (flapClamp) { Handler.SetupSoundSource(this.transform, flapClamp, "Foil Point", 80f, false, false, out clampSource); clampSource.volume = 1f; }
                        if (flapLoop) { Handler.SetupSoundSource(this.transform, flapLoop, "Loop Point", 80f, true, false, out loopSource); loopSource.volume = 1f; }
                    }
                }
            }
        }
    }











    // <summary>
    /// Calculate wing dimensions needed for analysis
    /// </summary>
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void AnalyseDimensions()
    {

        // ---------------------------------- Base Variables
        float tanc4 = Mathf.Tan((leadingEdgeSweep * Mathf.Deg2Rad)) + ((foilRootChord / (4 * foilSpan)) * (taperRatio - 1));
        quaterSweep = Mathf.Atan(tanc4) * Mathf.Rad2Deg;
        float tanc2 = tanc4 - ((2 / aspectRatio) * (0.25f * ((1 - taperRatio) / (1 + taperRatio))));
        tipCenterSweep = Mathf.Atan(tanc2) * Mathf.Rad2Deg;
        float tipDistance = Vector3.Distance(baseTip, TipChordLeading);
        leadingEdgeSweep = Mathf.Abs(Mathf.Atan(tipDistance / foilLength) * Mathf.Rad2Deg);
        groundAxis = new Vector3(0.0f, -1.0f, 0.0f); groundAxis.Normalize();


        // ------------------------------------ Calculate Dimensions
        foilRootChord = Vector3.Distance(RootChordTrailing, RootChordLeading);
        foilTipChord = Vector3.Distance(TipChordLeading, TipChordTrailing);
        foilMeanChord = MathBase.EstimateMeanChord(foilRootChord, foilTipChord);
        foilArea = MathBase.EstimatePanelArea(foilSpan, foilRootChord, foilTipChord);
        foilSpan = Mathf.Abs(foilLength);


        if (rootAirfoil && tipAirfoil)
        {
            float meanThickness = (rootAirfoil.maximumThickness + tipAirfoil.maximumThickness) / 2f;
            foilWettedArea = foilArea * (1.977f + (0.52f * meanThickness));
        }
        quaterRootChordPoint = MathBase.EstimateSectionPosition(RootChordLeading, RootChordTrailing, 0.25f);
        quaterTipChordPoint = MathBase.EstimateSectionPosition(TipChordLeading, TipChordTrailing, 0.25f);
        sweepCorrectionFactor = Mathf.Cos(quaterSweep * Mathf.Deg2Rad);
        if (foil != null && controlState == ControlType.Controllable) { AerofoilDesign.MapEfficiency(foil); }


        // ------------------------------------- Efficiency
        float eA = Mathf.Pow((Mathf.Tan(quaterSweep * Mathf.Deg2Rad)), 2);
        float eB = 4f + ((aspectRatio * aspectRatio) * (1 + eA));
        spanEfficiency = 2 / (2 - aspectRatio + Mathf.Sqrt(eB));
        aspectRatio = (foilSpan * foilSpan) / (foilArea);


        // ------------------------------------- Tip Factor
        if (tipDesign == WingtipDesign.Endplate) { effectiveChange = (wingTipHeight / foilSpan); }
        if (tipDesign == WingtipDesign.Square) { effectiveChange = 0.004f; }
        if (tipDesign == WingtipDesign.Spherical) { effectiveChange = -0.18f; }
        if (tipDesign == WingtipDesign.Winglet) { effectiveChange = (wingTipHeight / foilSpan); }
        if (tipDesign == WingtipDesign.Round) { effectiveChange = -0.19f; }


        // ------------------------------------- Surface Factor
        if (surfaceFinish == SurfaceFinish.MoldedComposite) { k = 0.17f; }
        if (surfaceFinish == SurfaceFinish.PaintedAluminium) { k = 3.33f; }
        if (surfaceFinish == SurfaceFinish.PolishedMetal) { k = 0.50f; }
        if (surfaceFinish == SurfaceFinish.ProductionSheetMetal) { k = 1.33f; }
        if (surfaceFinish == SurfaceFinish.SmoothPaint) { k = 2.08f; }
        MathBase.PlotSpoilerData(out spoilerDragCurve);


        // ------------------------------------- Sweep Factor
        if (sweepCorrectionMethod == SweepCorrection.RaymerJenkinson) { sweepCorrectionFactor = Mathf.Pow((Mathf.Cos(leadingEdgeSweep * Mathf.Deg2Rad)), 3); }
        else if (sweepCorrectionMethod == SweepCorrection.YoungLE) { sweepCorrectionFactor = Mathf.Cos(quaterSweep * Mathf.Deg2Rad); }
        else if (sweepCorrectionMethod == SweepCorrection.None) { sweepCorrectionFactor = 1; }
    }










    /// <summary>
    /// Recalculate vector points for the aerofoil shape based on transform position and set variables
    /// </summary>
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void AnalyseStructure()
    {
        // ---------------------------------- Base Factors
        Vector3 scaleFactor, supremeFactor, RootScale, tipPivot, rootPivot;
        float parentLength, parentWidth, combinedScale, wingTaper;

        // ----------------------------------- Variables
        if (connectedAircraft != null) { RootScale = connectedAircraft.transform.localScale; }
        else { RootScale = transform.root.localScale; }
        foilLength = transform.localScale.x; foilWidth = transform.localScale.z;
        wingTaper = 100 - taperPercentage;
        parentLength = RootScale.x; parentWidth = RootScale.z;


        // ----------------------------------- Dimension Ratios
        scaleFactor = transform.forward * (parentWidth * foilWidth * 0.5f); taperRatio = wingTaper / 100f;
        supremeFactor = transform.forward * ((parentWidth * foilWidth * 0.5f) * (wingTaper / 100));
        combinedScale = RootScale.magnitude * transform.localScale.magnitude;
        MathBase.EstimateFoilShapeProperties(twistString, sweepString, foilTwist, aerofoilSweepAngle, out trueTwist, out sweepAngle);


        // ------------------------------------- Center Points
        rootChordCenter = transform.position - (transform.right * (parentLength * foilLength * 0.5f));
        tipChordCenter = (transform.position + (transform.right * (parentLength * foilLength * 0.5f))) + (transform.forward * (sweepAngle / 90) * combinedScale);


        // ------------------------------------ Structure Points
        RootChordLeading = rootChordCenter + scaleFactor; RootChordTrailing = rootChordCenter - scaleFactor;
        baseTipLeading = tipChordCenter + supremeFactor; TipChordLeading = baseTipLeading + (transform.right * foilTipLeadingExtension);
        baseTipTrailing = tipChordCenter - supremeFactor; TipChordTrailing = baseTipTrailing - (foilTipTrailingExtension * transform.right);
        baseTip = (transform.position + (transform.right * (parentLength * foilLength * 0.5f))) + transform.forward * ((parentWidth * foilWidth * 0.5f));


        // ------------------------------------ Twist Reposition
        if (aerofoilType != AerofoilType.Stabilizer || stabilizerType == StabilizerType.Static) { stabilizerDeflection = 0f; }
        tipPivot = MathBase.EstimateSkewDistance(-stabilizerDeflection + trueTwist, TipChordLeading, TipChordTrailing, transform);
        rootPivot = MathBase.EstimateSkewDistance(-stabilizerDeflection, RootChordLeading, RootChordTrailing, transform);
        Vector3 basePivot = MathBase.EstimateSkewDistance(-stabilizerDeflection + trueTwist, baseTipLeading, baseTipTrailing, transform);


        baseTipTrailing = tipChordCenter - basePivot; baseTipLeading = tipChordCenter + basePivot;
        TipChordTrailing = tipChordCenter - (foilTipTrailingExtension * transform.right) - tipPivot;
        TipChordLeading = tipChordCenter + (transform.right * foilTipLeadingExtension) + tipPivot;
        RootChordTrailing = rootChordCenter - rootPivot; RootChordLeading = rootChordCenter + rootPivot;
        if (foil != null) { AerofoilDesign.MapWingPlate(foil); }
        aspectRatioCorrection = aspectRatio / (aspectRatio + 2 * (aspectRatio + 4) / (aspectRatio + 2));
    }









    /// <summary>
    /// Process sound emission for the flap and slat surfaces
    /// </summary>
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void AnalyseSound(int mode, float target)
    {
        // ------------------------ Set Volume
        if (aerofoilType == AerofoilType.Wing)
        {
            if (connectedAircraft != null && connectedAircraft.view != null)
            {
                if (connectedAircraft.view.cameraState == SilantroCamera.CameraState.Exterior) { loopSource.volume = 1f; clampSource.volume = 1f; }
                if (connectedAircraft.view.cameraState == SilantroCamera.CameraState.Interior) { loopSource.volume = 0.3f; clampSource.volume = 0.3f; }
            }
            else { loopSource.volume = 1f; clampSource.volume = 1f; }
        }

        // ------------------------ Move
        if (deflectionEngaged)
        {
            if (mode == 1) { if (baseFlap > target) { if (!loopSource.isPlaying) { loopSource.Play(); } } else { loopSource.Stop(); clampSource.PlayOneShot(flapClamp); deflectionEngaged = false; } }
            if (mode == 2) { if (baseFlap < target) { if (!loopSource.isPlaying) { loopSource.Play(); } } else { loopSource.Stop(); clampSource.PlayOneShot(flapClamp); deflectionEngaged = false; } }
        }
    }










    
    /// <summary>
    /// Process control inputs into surface deflection
    /// </summary>
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void AnalyseControl()
    {
        if (controlState == ControlType.Controllable && surfaceType != SurfaceType.Inactive)
        {
            float baseSpeed = coreSystem.currentSpeed;
            float flashRate = 1 / connectedAircraft.flightComputer.controlCylceRate;//Hz Refresh


            // ---------------------------------- Collect Inputs
            if (surfaceType == SurfaceType.Rudder) { baseInput = -yawInput; trimInput = -yawTrimInput; }
            if (surfaceType == SurfaceType.Aileron) { baseInput = rollInput; trimInput = rollTrimInput; }

            if (surfaceType == SurfaceType.Elevator)
            {
                float corePitch = pitchInput; trimInput = pitchTrimInput;
                if (transform.localScale.x < 0) { corePitch *= -1; trimInput *= -1f; }
                baseInput = corePitch;

                // -------------------------------------- Roll Coupled Stabilators
                float rollFactor = Mathf.Abs(rollInput); float pitchFactor = Mathf.Abs(pitchInput + pitchTrimInput);
                if (stabilatorType == StabilatorType.Coupled && rollFactor > 0.9f && pitchFactor < 0.4f && connectedAircraft.flightComputer.state == SilantroFlightComputer.CraftState.TakeoffLanding)
                {
                    if (aerofoilType == AerofoilType.Stabilator) { baseInput = ((corePitch * 2f) + (rollInput * (stabilatorCouplingPercentage / 100f) * 2f)) / 2f; }
                    if (aerofoilType == AerofoilType.Canard) { baseInput = ((corePitch * 2f) + (-rollInput * (stabilatorCouplingPercentage / 100f) * 2f)) / 2f; }
                }
            }

            if (surfaceType == SurfaceType.Ruddervator)
            {
                float basePitch = pitchInput; float baseYaw = -yawInput; if (transform.localScale.x < 0) { basePitch *= -1; }
                float baseTrimPitch = pitchTrimInput; float baseTrimYaw = -yawTrimInput; if (transform.localScale.x < 0) { baseTrimPitch *= -1; }
                baseInput = ((baseYaw * 2f) + (basePitch * 2f)) / 2f; trimInput = ((baseTrimYaw * 2f) + (baseTrimPitch * 2f)) / 2f;
            }

            if (surfaceType == SurfaceType.Elevon)
            {
                float basePitch = pitchInput; float baseRoll = rollInput; if (transform.localScale.x < 0) { basePitch *= -1; }
                float baseTrimPitch = pitchTrimInput; float baseTrimRoll = rollTrimInput; if (transform.localScale.x < 0) { baseTrimPitch *= -1; }
                baseInput = ((basePitch * 2f) + (baseRoll * 2f)) / 2f; trimInput = ((baseTrimRoll * 2f) + (baseTrimPitch * 2f)) / 2f;
            }



            // ----------------------------------- Clamp
            if (baseInput > 1) { baseInput = 1f; }
            if (trimInput > 1) { trimInput = 1f; }
            if (baseInput < -1) { baseInput = -1f; }
            if (trimInput < -1) { trimInput = -1f; }






            #region Trimmable Stabilizer

            if (aerofoilType == AerofoilType.Stabilizer && stabilizerType == StabilizerType.Trimmable)
            {
                if (transform.localScale.x < 0) { stabilizerDeflection = stabilizerInput > 0f ? stabilizerInput * negativeStabLimit : stabilizerInput * positiveStabLimit; }
                if (transform.localScale.x > 0) { stabilizerDeflection = stabilizerInput > 0f ? stabilizerInput * positiveStabLimit : stabilizerInput * negativeStabLimit; }
                if (stabilizerModel) { stabilizerModel.transform.localRotation = baseStabilizerRotation; stabilizerModel.transform.Rotate(stabilizerAxis, stabilizerDeflection); }
            }

            #endregion Trimmable Stabilizer

            // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- Surfaces
            #region Base Control Surface

            // ----------------------------------- Symmetric Deflection
            if (deflectionType == DeflectionType.Symmetric)
            {
                baseDeflection = baseInput > 0f ? baseInput * positiveLimit : baseInput * positiveLimit;
                if (trimState == TrimState.Available) { trimDeflection = trimInput > 0f ? trimInput * positiveTrimLimit : trimInput * positiveTrimLimit; }
                else { trimDeflection = 0f; }
                controlActuatorDeflection = baseDeflection + trimDeflection;
                if (controlActuatorDeflection > positiveLimit) { controlActuatorDeflection = positiveLimit; }
                if (controlActuatorDeflection < -positiveLimit) { controlActuatorDeflection = -positiveLimit; }
            }


            // ----------------------------------- Asymmetric Deflection
            if (deflectionType == DeflectionType.Asymmetric)
            {
                if (transform.localScale.x < 0)
                {
                    baseDeflection = baseInput > 0f ? baseInput * negativeLimit : baseInput * positiveLimit;
                    if (trimState == TrimState.Available) { trimDeflection = trimInput > 0f ? trimInput * negativeTrimLimit : trimInput * positiveTrimLimit; }
                    else { trimDeflection = 0f; }
                    controlActuatorDeflection = baseDeflection + trimDeflection;
                    if (controlActuatorDeflection > negativeLimit) { controlActuatorDeflection = negativeLimit; }
                    if (controlActuatorDeflection < -positiveLimit) { controlActuatorDeflection = -positiveLimit; }
                }
                if (transform.localScale.x > 0)
                {
                    baseDeflection = baseInput > 0f ? baseInput * positiveLimit : baseInput * negativeLimit;
                    if (trimState == TrimState.Available) { trimDeflection = trimInput > 0f ? trimInput * positiveTrimLimit : trimInput * negativeTrimLimit; }
                    else { trimDeflection = 0f; }
                    controlActuatorDeflection = baseDeflection + trimDeflection;
                    if (controlActuatorDeflection > positiveLimit) { controlActuatorDeflection = positiveLimit; }
                    if (controlActuatorDeflection < -negativeLimit) { controlActuatorDeflection = -negativeLimit; }
                }
            }

            if(surfaceType == SurfaceType.Aileron && aileronCouple == AileronCouple.Available)
            {
                if (connectedAircraft.isGrounded) { baseAileron = Mathf.MoveTowards(baseAileron, Mathf.Abs(flapDeflection), 30f * 0.65f * flashRate); }
                else { baseAileron = Mathf.MoveTowards(baseAileron, 0f, 30f * 0.65f * flashRate); }
              
                float baseAileronDeflection = 0f;
                if (wingPosition == WingPosition.Left) { baseAileronDeflection = baseAileron; }
                if (wingPosition == WingPosition.Right) { baseAileronDeflection = -baseAileron; }

                controlActuatorDeflection += baseAileronDeflection;
            }


            // ----------------------------------- Actuator Deflection
            if (controlArea > 0)
            {
                float surfaceEffectiveness = MathBase.EstimateControlEfficiency(baseSpeed + 1, controlArea, maximumControlTorque); currentControlEfficiency = surfaceEffectiveness;
                if (!float.IsNaN(surfaceEffectiveness)) { controlActuatorDeflection *= Mathf.Clamp01(surfaceEffectiveness); }
            }

            // ------------------------------------ Control Deflection
            if (aerofoilType == AerofoilType.Canard) { controlDeflection = Mathf.MoveTowards(controlDeflection, -controlActuatorDeflection, controlActuationSpeed * flashRate); }
            else
            {
                if (surfaceType == SurfaceType.Elevon || surfaceType == SurfaceType.Elevator) {  controlDeflection = Mathf.MoveTowards(controlDeflection, controlActuatorDeflection, controlActuationSpeed * flashRate); }
                else
                {
                    if (airbrakeActive && connectedAircraft.flightComputer.airbrakeType != SilantroFlightComputer.AirbrakeType.ActuatorOnly)
                    {
                        if (surfaceType == SurfaceType.Ruddervator) { if (transform.localScale.x > 0) { controlDeflection = -connectedAircraft.flightComputer.currentAileronSet; } 
                            else { controlDeflection = connectedAircraft.flightComputer.currentAileronSet; } }
                        if (surfaceType == SurfaceType.Aileron) { if (transform.localScale.x < 0) { controlDeflection = -connectedAircraft.flightComputer.currentAileronSet; }
                            else { controlDeflection = connectedAircraft.flightComputer.currentAileronSet; } }
                    }
                    else { controlDeflection = Mathf.MoveTowards(controlDeflection, controlActuatorDeflection, controlActuationSpeed * flashRate); }
                }
            }

            if (controlSurfaceModel) { controlSurfaceModel.transform.localRotation = baseControlRotation; controlSurfaceModel.transform.Rotate(controlAxis, controlDeflection); }
            if (dualControlSurfaceModel && baseSurfaceType == SurfaceModelType.Dual) { dualControlSurfaceModel.transform.localRotation = dualControlRotation; dualControlSurfaceModel.transform.Rotate(dualControlAxis, controlDeflection); }

            // ------------------------------------ Foil Correction
            if (transform.localScale.x < 0) { correctedDeflection = controlDeflection; }
            if (transform.localScale.x > 0) { correctedDeflection = -controlDeflection; }

            #endregion Base Control Surface

            #region Flap Control Surface

            if (aerofoilType == AerofoilType.Wing && flapState == ControlState.Active)
            {
                // ------------------------------------ Flap Deflection
                if (flapSetting > flapAngles.Count) { flapSetting = flapAngles.Count - 1; }
                if (flapSetting < 0) { flapSetting = 0; }

                baseFlap = Mathf.MoveTowards(baseFlap, flapAngles[flapSetting], flapActuationSpeed * 0.65f * flashRate);
                if (flapPosition == FlapPosition.Left) { baseFlapDeflection = baseFlap; }
                if (flapPosition == FlapPosition.Right) { baseFlapDeflection = -baseFlap; }

                if (transform.localScale.x > 0) { baseFlaperonDeflection = rollInput > 0f ? rollInput * negativeFlaperonLimit : rollInput * positiveFlaperonLimit; }
                if (transform.localScale.x < 0) { baseFlaperonDeflection = rollInput > 0f ? rollInput * positiveFlaperonLimit : rollInput * negativeFlaperonLimit; }


                // ------------------------------------- Flapevon
                float baseFlapevonPitch = pitchInput; float baseFlapevonRoll = rollInput; if (transform.localScale.x < 0) { baseFlapevonPitch *= -1; }
                baseFlapevonInput = ((baseFlapevonPitch * 2f) + (baseFlapevonRoll * 2f)) / 2f;
                if (transform.localScale.x > 0) { baseFlapevonDeflection = baseFlapevonInput > 0f ? baseFlapevonInput * negativeFlaperonLimit : baseFlapevonInput * positiveFlaperonLimit; }
                if (transform.localScale.x < 0) { baseFlapevonDeflection = baseFlapevonInput > 0f ? baseFlapevonInput * positiveFlaperonLimit : baseFlapevonInput * negativeFlaperonLimit; }

                // ------------------------------------- Flaperon
                if (flapType == FlapType.Flaperon) { flapActuatorDeflection = ((baseFlapDeflection * 2f) + (baseFlaperonDeflection * 2f)) / 2f; }
                else if (flapType == FlapType.Flapevon) { flapActuatorDeflection = ((baseFlapDeflection * 2f) + (baseFlapevonDeflection * 2f)) / 2f; }
                else { flapActuatorDeflection = baseFlapDeflection; }


                // ------------------------------------- Limits
                if (flapType == FlapType.Flaperon || flapType == FlapType.Flapevon)
                {
                    if (baseFlap > positiveFlaperonLimit) { posLimit = baseFlap; } else if (baseFlap < positiveFlaperonLimit) { posLimit = positiveFlaperonLimit; }
                    if (baseFlap > negativeFlaperonLimit) { negLimit = negativeFlaperonLimit; } else if (baseFlap < negativeFlaperonLimit) { negLimit = baseFlap; }
                    if (transform.localScale.x < 0) { if (flapActuatorDeflection > posLimit) { flapActuatorDeflection = posLimit; } }
                    else { if (flapActuatorDeflection < -posLimit) { flapActuatorDeflection = -posLimit; } }
                }

                // ----------------------------------- Flap Sounds
                if (loopSource != null && clampSource != null) { AnalyseSound(flapMode, flapAngles[flapSetting]); }


                // ----------------------------------- Speed Effect
                if (flapArea > 0)
                {
                    float flapEffectiveness = MathBase.EstimateControlEfficiency(baseSpeed + 1, flapArea, maximumFlapTorque); currentFlapEfficiency = flapEffectiveness;
                    if (!float.IsNaN(flapEffectiveness)) { flapActuatorDeflection *= Mathf.Clamp01(flapEffectiveness); }
                }

                // ------------------------------------ Control Deflection
                flapDeflection = Mathf.MoveTowards(flapDeflection, flapActuatorDeflection, flapActuationSpeed * flashRate);

                if (flapModelType == ModelType.Internal)
                {
                    if (flapSurfaceModel) { flapSurfaceModel.transform.localRotation = baseFlapRotation; flapSurfaceModel.transform.Rotate(flapAxis, flapDeflection); }
                    if (dualFlapSurfaceModel && flapSurfaceType == SurfaceModelType.Dual) { dualFlapSurfaceModel.transform.localRotation = dualFlapRotation; dualFlapSurfaceModel.transform.Rotate(dualFlapAxis, flapDeflection); }
                }
                else if (flapModelType == ModelType.Actuator)
                {
                    maxFlap = flapAngles.Max();
                    flapLevel = Mathf.Abs(flapDeflection) / maxFlap;
                    if (flapActuator != null) { flapActuator.targetActuationLevel = flapLevel; }
                }
            }

            #endregion Flap Control Surface

            #region Slat Control Surface

            if (aerofoilType == AerofoilType.Wing && slatState == ControlState.Active)
            {
                if (baseSlat > maximumSlatDeflection) { baseSlat = maximumSlatDeflection; }
                if (slatMovement == SlatMovement.Deflection) { slatDeflection = Mathf.MoveTowards(slatDeflection, baseSlat, flashRate * slatActuationSpeed); }
                if (slatMovement == SlatMovement.Extension) { slatPosition = Mathf.Lerp(slatPosition, baseSlat, flashRate * slatActuationSpeed); }

                if (slatModelType == ModelType.Internal)
                {
                    if (slatSurfaceModel)
                    {
                        //DEFLECTION
                        if (slatMovement == SlatMovement.Deflection) { slatSurfaceModel.transform.localRotation = baseSlatRotation; slatSurfaceModel.transform.Rotate(slatAxis, slatDeflection); }
                        if (slatMovement == SlatMovement.Deflection && dualSlatSurfaceModel != null) { dualSlatSurfaceModel.transform.localRotation = dualSlatRotation; dualSlatSurfaceModel.transform.Rotate(dualSlatAxis, slatDeflection); }
                        //SLIDING
                        if (slatMovement == SlatMovement.Extension) { slatSurfaceModel.transform.localPosition = baseSlatPosition; slatSurfaceModel.transform.localPosition += slatAxis * (slatPosition / 100); }
                        if (slatMovement == SlatMovement.Extension && dualSlatSurfaceModel != null) { dualSlatSurfaceModel.transform.localPosition = dualSlatPosition; dualSlatSurfaceModel.transform.localPosition += dualSlatAxis * (slatPosition / 100); }
                    }
                }
                else if (slatModelType == ModelType.Actuator)
                {
                    slatLevel = Mathf.Abs(slatDeflection) / maximumSlatDeflection;
                    if (slatActuator != null) { slatActuator.targetActuationLevel = slatLevel; }
                }
            }

            #endregion Slat Control Surface

            #region Spoiler Control Surface

            if (aerofoilType == AerofoilType.Wing && spoilerState == ControlState.Active)
            {
                // ------------------------------------ Spoiler Deflection
                if (transform.localScale.x > 0) { baseSpoilerDeflection = Mathf.MoveTowards(baseSpoilerDeflection, baseSpoiler, spoilerActuationSpeed * flashRate); }
                if (transform.localScale.x < 0) { baseSpoilerDeflection = Mathf.MoveTowards(baseSpoilerDeflection, -baseSpoiler, spoilerActuationSpeed * flashRate); }

                // ------------------------------------- Spoilereron
                if (spoilerType == SpoilerType.Spoileron)
                {
                    float rollableSpoiler = maximumSpoilerDeflection * (spoilerRollCoupling / 100f);

                    if (transform.localScale.x < 0) { baseSpoileronDeflection = rollInput > 0f ? 0 : rollInput * rollableSpoiler; }
                    if (transform.localScale.x > 0) { baseSpoileronDeflection = rollInput > 0f ? rollInput * rollableSpoiler : 0; }

                    spoilerActuatorDeflection = ((baseSpoilerDeflection * 2f) + (baseSpoileronDeflection * 2f)) / 2f;
                }
                else { spoilerActuatorDeflection = baseSpoilerDeflection; }

                if (transform.localScale.x > 0)
                {
                    if (spoilerActuatorDeflection > maximumSpoilerDeflection) { spoilerActuatorDeflection = maximumSpoilerDeflection; }
                    if (spoilerActuatorDeflection < 0) { spoilerActuatorDeflection = 0f; }
                }
                if (transform.localScale.x < 0)
                {
                    if (spoilerActuatorDeflection < -maximumSpoilerDeflection) { spoilerActuatorDeflection = -maximumSpoilerDeflection; }
                    if (spoilerActuatorDeflection > 0) { spoilerActuatorDeflection = 0f; }
                }



                // ----------------------------------- Speed Effect
                if (spoilerArea > 0)
                {
                    float spoilerEffectiveness = MathBase.EstimateControlEfficiency(baseSpeed + 1, spoilerArea, maximumSpoilerTorque); currentSpoilerEfficiency = spoilerEffectiveness;
                    if (!float.IsNaN(spoilerEffectiveness)) { spoilerActuatorDeflection *= Mathf.Clamp01(spoilerEffectiveness); }
                }

                // ------------------------------------ Control Deflection
                spoilerDeflection = Mathf.MoveTowards(spoilerDeflection, spoilerActuatorDeflection, spoilerActuationSpeed * flashRate);
                if (spoilerSurfaceModel) { spoilerSurfaceModel.transform.localRotation = baseSpoilerRotation; spoilerSurfaceModel.transform.Rotate(spoilerAxis, spoilerDeflection); }
                if (dualSpoilerSurfaceModel && spoilerSurfaceType == SurfaceModelType.Dual) { dualSpoilerSurfaceModel.transform.localRotation = dualSpoilerRotation; dualSpoilerSurfaceModel.transform.Rotate(dualSpoilerAxis, spoilerDeflection); }
            }

            #endregion Spoiler Control Surface
        }
    }












    public float vxcl, pxcl, vxcd;
    /// <summary>
    /// Process aerodynamic forces based on set variables
    /// </summary>
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    protected void AnalyseForces()
    {

        // ------------------------------------- Reset Variabless
        TotalBaseDrag = TotalControlDrag = TotalFlapDrag = TotalDrag = TotalLift = TotalSkinDrag = TotalMoment = 0f;
        vortexLiftPercentage = slatLiftCoefficient = flapLiftCoefficient = controlLiftCoefficient = 0f; vxcd = vxcl = 0f;
        slatDragCoefficient = flapDragCoefficient = baseLiftCoefficient = controlDragCoefficient = 0f;
        panelDragCoefficient = panelMomentCoefficient = panelLiftCoefficient = 0f;
        maximumAOA = -90f;

        // ------------------------------------- Panel Iteration
        for (int p = 0; p < foilSubdivisions; p++)
        {
            // ------------------------ Factors
            float currentPanel = (float)p; float nextPanel = (float)(p + 1);
            float panelCount = (float)foilSubdivisions;


            // ------------------------ Variables
            float currentPanelFactor = currentPanel / panelCount; float nextPanelFactor = nextPanel / panelCount;
            float panelWettedArea, panelArea, panelTipChord, panelRootChord, panelTaper, panelSpan, dynamicPressure, panelDistance, α, panelThickness, panelEdgeRadius;


            // ------------------------Points
            Vector3 LeadEdgeRight, LeadEdgeLeft, TrailEdgeRight, TrailEdgeLeft, geometricCenter, LeadCenter, TrailCenter, PanelCenter;
            Vector3 sectionRootCenter, sectionTipCenter, liftDirection, panelLiftForce, panelPitchTorque, panelDragForce, airflow, parallelFlow, normalWind;
            Vector3 leftExtension, rightExtension, leftFlapExtension, rightFlapExtension;


            LeadEdgeRight = MathBase.EstimateSectionPosition(RootChordLeading, baseTipLeading, nextPanelFactor);
            LeadEdgeLeft = MathBase.EstimateSectionPosition(RootChordLeading, baseTipLeading, currentPanelFactor);
            TrailEdgeLeft = MathBase.EstimateSectionPosition(RootChordTrailing, baseTipTrailing, currentPanelFactor);
            TrailEdgeRight = MathBase.EstimateSectionPosition(RootChordTrailing, baseTipTrailing, nextPanelFactor);


            if (controlState == ControlType.Controllable && surfaceType != SurfaceType.Inactive && availableControls != AvailableControls.SecondaryOnly && controlSections[p] == true && controlAnalysis != AnalysisMethod.NumericOnly)
            {
                FMath.EstimateControlExtension(LeadEdgeLeft, TrailEdgeRight, TrailEdgeLeft, LeadEdgeRight, controlRootChord, controlTipChord, controlDeflection, out leftExtension, out rightExtension);
                TrailEdgeLeft = MathBase.EstimateSectionPosition(TrailEdgeLeft, LeadEdgeLeft, (controlRootChord * 0.01f)) + leftExtension;
                TrailEdgeRight = MathBase.EstimateSectionPosition(TrailEdgeRight, LeadEdgeRight, (controlTipChord * 0.01f)) + rightExtension;
            }


            if (controlState == ControlType.Controllable && flapState == ControlState.Active && aerofoilType == AerofoilType.Wing && flapSections[p] == true && flapAnalysis != AnalysisMethod.NumericOnly)
            {
                FMath.EstimateControlExtension(LeadEdgeLeft, TrailEdgeRight, TrailEdgeLeft, LeadEdgeRight, flapRootChord, flapTipChord, flapDeflection, out leftFlapExtension, out rightFlapExtension);
                TrailEdgeLeft = MathBase.EstimateSectionPosition(TrailEdgeLeft, LeadEdgeLeft, (flapRootChord * 0.01f)) + leftFlapExtension;
                TrailEdgeRight = MathBase.EstimateSectionPosition(TrailEdgeRight, LeadEdgeRight, (flapTipChord * 0.01f)) + rightFlapExtension;
            }


            geometricCenter = MathBase.EstimateGeometricCenter(LeadEdgeRight, TrailEdgeRight, LeadEdgeLeft, TrailEdgeLeft);
            LeadCenter = MathBase.EstimateSectionPosition(LeadEdgeLeft, LeadEdgeRight, 0.5f); TrailCenter = MathBase.EstimateSectionPosition(TrailEdgeLeft, TrailEdgeRight, 0.5f);
            PanelCenter = LeadCenter - TrailCenter; PanelCenter.Normalize();
            panelTipChord = Vector3.Distance(LeadEdgeRight, TrailEdgeRight); panelRootChord = Vector3.Distance(LeadEdgeLeft, TrailEdgeLeft);
            sectionRootCenter = MathBase.EstimateSectionPosition(LeadEdgeLeft, TrailEdgeLeft, 0.5f); sectionTipCenter = MathBase.EstimateSectionPosition(LeadEdgeRight, TrailEdgeRight, 0.5f);



            // ---------------------------------- Panel Dimensions
            panelArea = MathBase.EstimatePanelSectionArea(LeadEdgeLeft, LeadEdgeRight, TrailEdgeLeft, TrailEdgeRight);
            panelTaper = panelTipChord / panelRootChord;
            panelSpan = Vector3.Distance(sectionRootCenter, sectionTipCenter);
            panelDistance = Vector3.Distance(RootChordTrailing, TrailEdgeLeft) / foilSpan;
            float yM = (panelSpan / 6) * ((1 + (2 * panelTaper)) / (1 + panelTaper)); float panelFactor = yM / panelSpan;
            Vector3 yMGCTop = MathBase.EstimateSectionPosition(TrailEdgeLeft, LeadEdgeLeft, (1 - panelFactor));
            Vector3 yMGCBottom = MathBase.EstimateSectionPosition(TrailEdgeRight, LeadEdgeRight, (1 - panelFactor));
            float effectiveThickness = MathBase.EstimateEffectiveValue(rootAirfoil.maximumThickness, tipAirfoil.maximumThickness, panelDistance, 0, foilSpan);
            panelWettedArea = panelArea * (1.977f + (0.52f * effectiveThickness));
            float piFactor = (6.28f / (aspectRatio * 3.142f));
            if (aspectRatio < 4) { Asf = (Mathf.Sqrt((1 - Mathf.Pow(coreSystem.machSpeed, 2)) + Mathf.Pow(piFactor, 2)) + piFactor); }
            else { Asf = (Mathf.Sqrt((1 - Mathf.Pow(coreSystem.machSpeed, 2))) + piFactor); }
            panelThickness = MathBase.EstimateEffectiveValue(rootAirfoil.maximumThickness, tipAirfoil.maximumThickness, panelDistance, 0, foilSpan);
            panelEdgeRadius = MathBase.EstimateEffectiveValue(rootAirfoil.leadingEdgeRadius, tipAirfoil.leadingEdgeRadius, panelDistance, 0, foilSpan);



            // ---------------------------------- AOA
            Vector3 panelWorldFlow = connectedAircraft.aircraft.GetPointVelocity(geometricCenter); airflow = -panelWorldFlow;
            Vector3 circulation = Vector3.Cross(connectedAircraft.aircraft.angularVelocity.normalized, (geometricCenter - connectedAircraft.aircraft.worldCenterOfMass).normalized);
            circulation *= -((connectedAircraft.aircraft.angularVelocity.magnitude) * (geometricCenter - connectedAircraft.aircraft.worldCenterOfMass).magnitude);
            airflow += circulation; parallelFlow = (transform.right * (Vector3.Dot(transform.right, airflow))); airflow -= parallelFlow; normalWind = airflow.normalized;



            // ----------- Extract alpha and calculate lift Direction
            if (liftMethod == LiftMethod.Linear)
            {
                Vector3 panelVelocity = transform.InverseTransformDirection(panelWorldFlow);
                α = Vector3.Angle(Vector3.forward, panelVelocity) * -Mathf.Sign(panelVelocity.y);
                liftDirection = Vector3.Cross(connectedAircraft.aircraft.velocity, transform.right).normalized;
            }
            else
            {
                α = Mathf.Acos(Vector3.Dot(PanelCenter, -normalWind)) * Mathf.Rad2Deg;
                liftDirection = Vector3.Cross(PanelCenter, (yMGCBottom - yMGCTop).normalized);
                liftDirection.Normalize(); if (transform.localScale.x < 0.0f) { liftDirection = -liftDirection; }
                if (Vector3.Dot(liftDirection, normalWind) < 0.0f) { α = -α; }
            }


            //---- Filter
            if (α > 89) { α = 89f; }
            if (α < -89) { α = -89f; }
            if (float.IsNaN(α)) { α = 0f; }
            if (float.IsInfinity(α)) { α = 0f; }
            if (α > maximumAOA) { maximumAOA = α; }
            if (p == 0) { rootAlpha = α; }
            if (p == foilSubdivisions - 1) { tipAlpha = α; }



            // ---------------------------------- Panel Dynamics
            groundInfluenceFactor = 1f; if (effectState == GroundEffectState.Consider)
            { groundInfluenceFactor = MathBase.EstimateGroundEffectFactor(transform, groundAxis, geometricCenter, foilSpan, groundLayer); }
            dynamicPressure = 0.5f * coreSystem.airDensity * Mathf.Pow(airflow.magnitude, 2);
            AnalyseCoefficients(α, panelDistance, groundInfluenceFactor, out baseLiftCoefficient, out baseDragCoefficient, out panelMomentCoefficient);
            float panelLiftSlope = MathBase.EstimateEffectiveValue(rootAirfoil.centerLiftSlope, tipAirfoil.centerLiftSlope, panelSpan, 0, foilSpan);
            if (panelLiftSlope < 1) { panelLiftSlope = 6.28f; }
            panelDragCoefficient = baseDragCoefficient;


            //1.0 ---------------------------------- Vortex Lift
            if (aerofoilType == AerofoilType.Wing && vortexState == VortexLift.Consider)
            {
                float αr = α * Mathf.Deg2Rad;
                float potentialLift = baseLiftCoefficient * Mathf.Sin(αr) * Mathf.Cos(αr) * Mathf.Cos(αr);
                float ratioFactor = (spanEfficiency * baseLiftCoefficient * baseLiftCoefficient) / (3.142f * aspectRatio);
                float vortexBase = (baseLiftCoefficient - ratioFactor) * (1 / (Mathf.Cos(leadingEdgeSweep * Mathf.Deg2Rad))) * Mathf.Sin(αr) * Mathf.Sin(αr) * Mathf.Cos(αr);
                float vortexLiftCoefficient = potentialLift + vortexBase;
                float vortexDragCoefficient = vortexLiftCoefficient * Mathf.Tan(αr);

                panelLiftCoefficient = baseLiftCoefficient + vortexLiftCoefficient; vxcl = vortexLiftCoefficient;
                panelDragCoefficient += vortexDragCoefficient; vxcd = vortexDragCoefficient;
            }
            else { panelLiftCoefficient = baseLiftCoefficient; }



            //1.1 ---------------------------------- Slat Numeric Lift
            if (aerofoilType == AerofoilType.Wing && slatState == ControlState.Active && slatSections[p] == true)
            {
                float effectiveSlatChord = MathBase.EstimateEffectiveValue(slatRootChord, slatTipChord, panelDistance, 0, slatSpan);
                float dCldδ = liftDeltaCurve.Evaluate(effectiveSlatChord / 100);
                float ƞmax = nMaxCurve.Evaluate(panelEdgeRadius / (100 * panelThickness));
                float ƞδ = nDeltaCurve.Evaluate(slatDeflection);
                float panelSlatLiftCoefficient = dCldδ * ƞmax * ƞδ * slatDeflection;

                if (wingType == WingType.Conventional) { slatLiftCoefficient += panelSlatLiftCoefficient; panelLiftCoefficient += panelSlatLiftCoefficient; }
                else { if (maximumAOA > stallAOA) { slatLiftCoefficient += panelSlatLiftCoefficient; panelLiftCoefficient += panelSlatLiftCoefficient; } }
            }



            //1.2 ---------------------------------- Flap Numeric Lift
            if (aerofoilType == AerofoilType.Wing && flapState == ControlState.Active && flapSections[p] == true && flapAnalysis != AnalysisMethod.GeometricOnly)
            {
                float effectiveFlapChord = MathBase.EstimateEffectiveValue(flapRootChord, flapTipChord, panelDistance, 0, flapSpan);
                float Δclmax = clBaseCurve.Evaluate(panelThickness * 100f);
                float k1 = k1Curve.Evaluate(effectiveFlapChord);
                float numericDeflection = Mathf.Abs(flapDeflection);
                float k2 = k2Curve.Evaluate(numericDeflection);
                float k3 = k3Curve.Evaluate(numericDeflection / 60f);

                if (flapCorrectionMethod == NumericCorrection.KhanNahon)
                {
                    float θf = Mathf.Acos(2 * (effectiveFlapChord / 100) - 1);
                    float τ = 1 - (θf - Mathf.Sin(θf)) / Mathf.PI;
                    float ƞ = effectivenessPlot.Evaluate(numericDeflection);
                    panelFlapLiftCoefficient = Mathf.Abs(panelLiftSlope) * τ * ƞ * numericDeflection * Mathf.Deg2Rad * Mathf.Sign(flapDeflection);
                }
                if (flapCorrectionMethod == NumericCorrection.DATCOM) { panelFlapLiftCoefficient = k1 * k2 * k3 * Δclmax * Mathf.Sign(flapDeflection); }
                if (flapPosition == FlapPosition.Right) { panelFlapLiftCoefficient *= -1f; }
                panelFlapDragCoefficient = MathBase.EstimateDeflectionDrag(effectiveFlapChord, numericDeflection, 1, panelArea, foilArea);


                flapLiftCoefficient += panelFlapLiftCoefficient;
                flapDragCoefficient += panelFlapDragCoefficient;
                panelLiftCoefficient += panelFlapLiftCoefficient;
                panelDragCoefficient += panelFlapDragCoefficient;
            }


            //1.3 ---------------------------------- Control Numeric Lift
            if (aerofoilType != AerofoilType.Balance)
            {
                if (controlState == ControlType.Controllable && surfaceType != SurfaceType.Inactive && availableControls != AvailableControls.SecondaryOnly && controlSections[p] == true && controlAnalysis != AnalysisMethod.GeometricOnly)
                {
                    float effectiveControlChord = MathBase.EstimateEffectiveValue(controlRootChord, controlTipChord, panelDistance, 0, controlSpan);
                    float Δclmax = clBaseCurve.Evaluate(panelThickness * 100f);
                    float k1 = k1Curve.Evaluate(effectiveControlChord);
                    float numericDeflection = Mathf.Abs(controlDeflection);
                    float k2 = k2Curve.Evaluate(numericDeflection);
                    float k3 = k3Curve.Evaluate(numericDeflection / 60f);
                    float panelControlLiftCoefficient = 0f;

                    if (controlCorrectionMethod == NumericCorrection.DATCOM) { panelControlLiftCoefficient = k1 * k2 * k3 * Δclmax * Mathf.Sign(controlDeflection); }
                    if (controlCorrectionMethod == NumericCorrection.KhanNahon)
                    {
                        float θf = Mathf.Acos(2 * (effectiveControlChord / 100) - 1);
                        float τ = 1 - (θf - Mathf.Sin(θf)) / Mathf.PI;
                        float ƞ = effectivenessPlot.Evaluate(numericDeflection);
                        panelControlLiftCoefficient = Mathf.Abs(panelLiftSlope) * τ * ƞ * numericDeflection * Mathf.Deg2Rad * Mathf.Sign(controlDeflection);
                    }
                    if (transform.localScale.x > 0) { panelControlLiftCoefficient *= -1f; }
                    float panelControlDragCoefficient = MathBase.EstimateDeflectionDrag(effectiveControlChord, numericDeflection, 1, panelArea, foilArea);


                    controlDragCoefficient += panelControlDragCoefficient;
                    controlLiftCoefficient += panelControlLiftCoefficient;
                    panelLiftCoefficient += panelControlLiftCoefficient;
                    panelDragCoefficient += panelControlDragCoefficient;
                }
            }



            //1.4 ------------------------------------------------------------------ $$LIFT
            if (aerofoilType == AerofoilType.Wing && controlState == ControlType.Controllable && spoilerState == ControlState.Active && spoilerSections[p] == true) { spoilerFactor = (maximumSpoilerDeflection - Mathf.Abs(spoilerDeflection)) / maximumSpoilerDeflection; } else { spoilerFactor = 1f; }
            float panelLift = sweepCorrectionFactor * spoilerFactor * panelArea * panelLiftCoefficient * dynamicPressure; TotalLift += panelLift; pxcl = panelLiftCoefficient; vortexLiftPercentage = vxcl / pxcl;
            if (liftMethod == LiftMethod.Corrected) { panelLiftForce = Vector3.Cross(transform.right, airflow); panelLiftForce.Normalize(); panelLiftForce *= panelLift; }
            else { panelLiftForce = liftDirection * panelLift; }


            //2. ------------------------------------------------------------------ $$DRAG

            //-----------------------------------------Wave Drag
            float rootMcr = rootAirfoil.Mcr; float tipMcr = tipAirfoil.Mcr;
            float panelMcr = MathBase.EstimateEffectiveValue(rootMcr, tipMcr, panelSpan, 0, foilSpan);
            if (panelMcr < 0.5f) { panelMcr = ((rootMcr * 2) + (tipMcr * 2)) / 2f; }
            float Mcrit = panelMcr - (0.1f * panelLiftCoefficient);
            float M_Mcr = 1 - Mcrit;
            float cdw0 = 0.0264f;//20f * Mathf.Pow(M_Mcr, 4f);

            float δm = 1 - Mcrit;
            float xf = (-8f * (coreSystem.machSpeed - (1 - (0.5f * δm)))) / δm;
            float fmx = Mathf.Exp(xf);
            float fm = 1 / (1 + fmx);
            float kdw = 0.5f;
            float kdwm = 0.05f;
            float dx = Mathf.Pow((Mathf.Pow((coreSystem.machSpeed - kdwm), 2f) - 1), 2) + Mathf.Pow(kdw, 4);
            float correction = Mathf.Pow(Mathf.Cos(quaterSweep * Mathf.Deg2Rad), 2.5f);
            float cdw = (fm * cdw0 * kdw) / (Mathf.Pow(dx, 0.25f)) * correction;
            if (cdw < 0) { cdw = 0f; }


            // ----- Induced Drag
            float correctedLift = panelLiftCoefficient / Asf;
            float inducedDragCoefficient = (correctedLift * correctedLift) / (3.142f * (aspectRatio) * spanEfficiency);
            if(aerofoilType == AerofoilType.Balance) { panelDragCoefficient += inducedDragCoefficient; } else { panelDragCoefficient += (inducedDragCoefficient + cdw); }
          
            float baseDrag = panelArea * panelDragCoefficient * dynamicPressure * Mathf.Cos(leadingEdgeSweep * Mathf.Deg2Rad);
            float controlDrag = controlArea * controlDragCoefficient * dynamicPressure * Mathf.Cos(leadingEdgeSweep * Mathf.Deg2Rad);
            float flapDrag = flapArea * flapDragCoefficient * dynamicPressure * Mathf.Cos(leadingEdgeSweep * Mathf.Deg2Rad);
            TotalBaseDrag += baseDrag; TotalFlapDrag += flapDrag; TotalControlDrag += controlDrag;

            //------Skin Drag
            float Cf = MathBase.EstimateSkinDragCoefficient(panelRootChord, panelTipChord, (MathBase.ConvertSpeed(airflow.magnitude, "KTS")), coreSystem.airDensity, coreSystem.viscocity, coreSystem.machSpeed, k);
            float panelSkinDrag = dynamicPressure * panelWettedArea * Cf; TotalSkinDrag += panelSkinDrag;

            // ----- Spoiler Drag
            float panelSpoilerDrag = 0f;
            if (aerofoilType == AerofoilType.Wing && controlState == ControlType.Controllable && spoilerState == ControlState.Active && spoilerSections[p] == true)
            {
                spoilerFactor = (maximumSpoilerDeflection - Mathf.Abs(spoilerDeflection)) / maximumSpoilerDeflection;
                spoilerFactor = Mathf.Clamp(spoilerFactor, 0, 1); float spoilerAngle = 90 - Mathf.Abs(spoilerDeflection);
                panelSpoilerDrag = ((spoilerArea) / spoilerSections.Length) * spoilerDragFactor * dynamicPressure * Mathf.Cos(spoilerAngle * Mathf.Deg2Rad);
            }
            float panelDrag = (baseDrag + panelSkinDrag + panelSpoilerDrag);
            panelDragForce = airflow; panelDragForce.Normalize(); panelDragForce *= panelDrag; TotalDrag += panelDrag;
            vortexDragPercentage = vxcd / (panelDragCoefficient + panelDragCoefficient + flapDragCoefficient);


           

            //3. ------------------------------------------------------------------ $$MOMENT
            panelMomentCoefficient *= aspectRatio / (aspectRatio + 4f);
            float panelMeanChord = MathBase.EstimateMeanChord(panelRootChord, panelTipChord);
            float panelPitchMoment = dynamicPressure * panelArea * panelMeanChord * panelMeanChord * panelMomentCoefficient; TotalMoment += panelPitchMoment;
            panelPitchTorque = Vector3.Cross(PanelCenter, panelLiftForce.normalized); panelPitchTorque.Normalize(); panelPitchTorque *= panelPitchMoment;


            // -------------------------------------------- Apply
            if (!float.IsNaN(panelLift) && !float.IsInfinity(panelLift)) { connectedAircraft.aircraft.AddForceAtPosition(panelLiftForce, geometricCenter, ForceMode.Force); }
            if (!float.IsNaN(panelDrag) && !float.IsInfinity(panelDrag)) { connectedAircraft.aircraft.AddForceAtPosition(panelDragForce, geometricCenter, ForceMode.Force); }
            if (!float.IsNaN(panelPitchMoment) && !float.IsInfinity(panelPitchMoment)) { connectedAircraft.aircraft.AddTorque(panelPitchTorque, ForceMode.Force); }


            // -------------------------------------------- Debug
            if (debugMode == DebugMode.Active)
            {
                panelAOA[p] = α;
                panelCL[p] = panelLiftCoefficient;
                panelDragForces[p] = panelDrag;
                panelLiftForces[p] = panelLift;
                panelMoments[p] = panelPitchMoment;
            }
        }


        // -------------------------------------------- Debug
        if (slatState == ControlState.Active) { slatLiftCoefficient /= slatSections.Length; }
        if (flapState == ControlState.Active) { flapCoefficient = flapDragCoefficient / flapSections.Length; }
        if (controlState == ControlType.Controllable) { controlCoefficient = controlDragCoefficient / controlSections.Length; }
    }
   











    /// <summary>
    /// Process aerodynamic coefficients
    /// </summary>
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    protected void AnalyseCoefficients(float angleOfAttack, float panelSpan, float groundFactor, out float liftCoefficient, out float dragCoefficient, out float momentCoefficient)
    {

        //1. --------------------------------------------- Lift
        float rootLiftCoefficient = rootAirfoil.liftCurve.Evaluate(angleOfAttack);
        float tipLiftCoefficient = tipAirfoil.liftCurve.Evaluate(angleOfAttack);
        liftCoefficient = MathBase.EstimateEffectiveValue(rootLiftCoefficient, tipLiftCoefficient, panelSpan, 0, foilSpan);
        if (effectState == GroundEffectState.Consider) { liftCoefficient /= Mathf.Sqrt(groundFactor); }


        //2. --------------------------------------------- Drag
        float rootDragCoefficient = rootAirfoil.dragCurve.Evaluate(angleOfAttack);
        float tipDragoefficient = tipAirfoil.dragCurve.Evaluate(angleOfAttack);
        dragCoefficient = MathBase.EstimateEffectiveValue(rootDragCoefficient, tipDragoefficient, panelSpan, 0, foilSpan);


        //3. --------------------------------------------- Moment
        float rootMomentCoefficient = rootAirfoil.momentCurve.Evaluate(angleOfAttack);
        float tipMomentoefficient = tipAirfoil.momentCurve.Evaluate(angleOfAttack);
        momentCoefficient = MathBase.EstimateEffectiveValue(rootMomentCoefficient, tipMomentoefficient, panelSpan, 0, foilSpan);
    }






    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void FixedUpdate()
    {
        if (allOk && connectedAircraft.isControllable)
        {
            // --------------------- Shape
            AnalyseStructure();

            // --------------------- Control
            AnalyseControl();

            // --------------------- Forces
            AnalyseForces();
        }
    }





#if UNITY_EDITOR
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        if (foil == null) { foil = this.GetComponent<SilantroAerofoil>(); }

        if (sweepString != sweepDirection.ToString()) { sweepString = sweepDirection.ToString(); }
        if (twistString != twistDirection.ToString()) { twistString = twistDirection.ToString(); }

        // ------------------------ Base Shape
        AnalyseDimensions(); AerofoilDesign.ShapeAerofoilPlain(foil);
    }
#endif



#if UNITY_EDITOR
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void OnDrawGizmos()
    {

        // ------------------------ Collect Foil
        if (foil == null) { foil = this.GetComponent<SilantroAerofoil>(); }

        AnalyseStructure(); if (!Application.isPlaying) { AerofoilDesign.ShapeAerofoil(foil); }

        // ------------------------ Advanced Visuals
        if (controlState == ControlType.Controllable) { AerofoilDesign.MapControlSurface(foil); }
    }
#endif
}

using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif




namespace Oyedoyin
{
    public class Handler
    {



        /// <summary>
        /// Creates a new sound source for the selected component.
        /// </summary>
        /// <param name="parentObject">Object to make the sound source a child of.</param>
        /// <param name="soundClip">Audio clip to play from the sound source.</param>
        /// <param name="name">Sound source label.</param>
        /// <param name="fallOffDistance">Maximum reach distance of the sound source.</param>
        /// <param name="loopSound">Determines if the sound loops or not e.g engine Idle loops, but Ignition sound is played once.</param>
        /// <param name="playOnCreate">Only used by continous playing sound sources.</param>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void SetupSoundSource(Transform parentObject, AudioClip soundClip, string name, float fallOffDistance, bool loopSound, bool playOnCreate, out AudioSource soundSource)
        {
            //CREATE SOURCE HOLDER
            GameObject source = new GameObject(name);
            source.transform.parent = parentObject; source.transform.localPosition = Vector3.zero;
            soundSource = source.AddComponent<AudioSource>();

            //CONFIGURE PROPERTIES
            soundSource.clip = soundClip; soundSource.volume = 0f;
            soundSource.loop = loopSound; soundSource.spatialBlend = 1f; soundSource.dopplerLevel = 0f;
            soundSource.rolloffMode = AudioRolloffMode.Custom; soundSource.maxDistance = fallOffDistance;
            soundSource.playOnAwake = false;
            if (playOnCreate) { soundSource.Play(); }
        }









        /// <summary>
        /// Prepares the mesh for component usage by setting up the required vector rotation/movement
        /// </summary>
        /// <param name="deflectionDirection"> Mesh rotation direction either CW or CCW.</param>
        /// <param name="axis">Mesh rotation axis.</param>
        /// <returns>
        /// Rotation axis vector3
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector3 EstimateModelProperties(string deflectionDirection, string axis)
        {
            Vector3 defaultAxis = new Vector3(0, 0, 0); if (deflectionDirection == "CCW")
            {
                if (axis == "X") { defaultAxis = new Vector3(-1, 0, 0); } else if (axis == "Y") { defaultAxis = new Vector3(0, -1, 0); } else if (axis == "Z") { defaultAxis = new Vector3(0, 0, -1); }
            }
            else { if (axis == "X") { defaultAxis = new Vector3(1, 0, 0); } else if (axis == "Y") { defaultAxis = new Vector3(0, 1, 0); } else if (axis == "Z") { defaultAxis = new Vector3(0, 0, 1); } }
            //RETURN
            defaultAxis.Normalize(); return defaultAxis;
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public class Axis
        {
            public string name = String.Empty;
            public string descriptiveName = String.Empty;
            public string descriptiveNegativeName = String.Empty;
            public string negativeButton = String.Empty;
            public string positiveButton = String.Empty;
            public string altNegativeButton = String.Empty;
            public string altPositiveButton = String.Empty;
            public float gravity = 0.0f;
            public float dead = 0.001f;
            public float sensitivity = 1.0f;
            public bool snap = false;
            public bool invert = false;
            public int type = 2;
            public int axis = 0;
            public int joyNum = 0;
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void ControlAxis(Axis axis, bool duplicate)
        {
#if UNITY_EDITOR
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

            SerializedProperty axisIter = axesProperty.Copy();
            axisIter.Next(true);
            axisIter.Next(true);
            while (axisIter.Next(false))
            {
                SerializedProperty desName = axisIter.FindPropertyRelative("descriptiveName");
                SerializedProperty name = axisIter.FindPropertyRelative("m_Name");
                if (desName != null && name != null)
                {
                    string desNameString = axisIter.FindPropertyRelative("descriptiveName").stringValue;
                    string nameString = axisIter.FindPropertyRelative("m_Name").stringValue;
                    if (desNameString == axis.descriptiveName) { return; }
                    else if (nameString == axis.name && !duplicate) { return; }
                }
            }

            axesProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();

            SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);
            axisProperty.FindPropertyRelative("m_Name").stringValue = axis.name;
            axisProperty.FindPropertyRelative("descriptiveName").stringValue = axis.descriptiveName;
            axisProperty.FindPropertyRelative("descriptiveNegativeName").stringValue = axis.descriptiveNegativeName;
            axisProperty.FindPropertyRelative("negativeButton").stringValue = axis.negativeButton;
            axisProperty.FindPropertyRelative("positiveButton").stringValue = axis.positiveButton;
            axisProperty.FindPropertyRelative("altNegativeButton").stringValue = axis.altNegativeButton;
            axisProperty.FindPropertyRelative("altPositiveButton").stringValue = axis.altPositiveButton;
            axisProperty.FindPropertyRelative("gravity").floatValue = axis.gravity;
            axisProperty.FindPropertyRelative("dead").floatValue = axis.dead;
            axisProperty.FindPropertyRelative("sensitivity").floatValue = axis.sensitivity;
            axisProperty.FindPropertyRelative("snap").boolValue = axis.snap;
            axisProperty.FindPropertyRelative("invert").boolValue = axis.invert;
            axisProperty.FindPropertyRelative("type").intValue = axis.type;
            axisProperty.FindPropertyRelative("axis").intValue = axis.axis;
            axisProperty.FindPropertyRelative("joyNum").intValue = axis.joyNum;
            serializedObject.ApplyModifiedProperties();
#endif
        }
    }







    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Contains special type of Gain Vetor to be used in gain scheduling
    /// </summary>
    [System.Serializable]
    public class Gain
    {
        [HideInInspector] public float Kp, Ki, Kd;
        [HideInInspector] public float minimum, maximum;
        [HideInInspector] public float factor;
        [HideInInspector] public string gainLabel;
        public enum FactorType { Mach, KnotSpeed }
        [HideInInspector] public FactorType factorType = FactorType.Mach;
    }


    /// <summary>
    /// Contains a single PID data set (Proportional, Integral and Differential)
    /// </summary>
    [System.Serializable]
    public class GainVector
    {
        public float Kp, Ki, Kd;

        public GainVector() { }

        public GainVector(float kp, float ki, float kd)
        {
            Kp = kp;
            Ki = ki;
            Kd = kd;
        }
    }



    [System.Serializable]
    public class SilantroPath
    {
        // ------------------------ Variables
        public float pathLength, lengthA, lengthB, lengthC;
        //private Vector3 tangentA, tangentB;
        public List<Vector3> pathVectorPoints;
        public bool segment2Turning;
        public bool segment1TurningRight, segment2TurningRight, segment3TurningRight;

        public SilantroPath() { }

        public SilantroPath(float length1, float length2, float length3, Vector3 tangent1, Vector3 tangent2)
        {
            this.pathLength = length1 + length2 + length3;

            this.lengthA = length1;
            this.lengthB = length2;
            this.lengthC = length3;

            //this.tangentA = tangent1; this.tangentB = tangent2;
        }


        // -------------------------------------------------------------- Turn State
        public void SetIfTurningRight(bool segment1TurningRight, bool segment2TurningRight, bool segment3TurningRight)
        {
            this.segment1TurningRight = segment1TurningRight;
            this.segment2TurningRight = segment2TurningRight;
            this.segment3TurningRight = segment3TurningRight;
        }
    }











    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Silantro math class for internal calculations.
    /// </summary>
    public static class MathBase
    {

        public static void m_plot_atm(out AnimationCurve pressure, out AnimationCurve temp, out AnimationCurve density)
        {

            // ---------------------------------- Pressure
            pressure = new AnimationCurve();
            pressure.AddKey(new Keyframe(-0.044f, 110.1f));
            pressure.AddKey(new Keyframe(0.179f, 97.18f));
            pressure.AddKey(new Keyframe(0.966f, 84.89f));
            pressure.AddKey(new Keyframe(2.189f, 71.05f));
            pressure.AddKey(new Keyframe(3.629f, 59.27f));
            pressure.AddKey(new Keyframe(5.242f, 48.85f));
            pressure.AddKey(new Keyframe(6.941f, 38.60f));
            pressure.AddKey(new Keyframe(11.12f, 30.91f));
            pressure.AddKey(new Keyframe(13.95f, 22.03f));
            pressure.AddKey(new Keyframe(16.95f, 10.41f));
            pressure.AddKey(new Keyframe(20.21f, 6.491f));
            pressure.AddKey(new Keyframe(23.91f, 3.758f));
            pressure.AddKey(new Keyframe(26.99f, 2.562f));
            pressure.AddKey(new Keyframe(29.82f, 2.220f));



            // ---------------------------------- Temperature
            temp = new AnimationCurve();
            temp.AddKey(new Keyframe(-0.284f, 16.70f));
            temp.AddKey(new Keyframe(0.652f, 9.9500f));
            temp.AddKey(new Keyframe(1.993f, -0.250f));
            temp.AddKey(new Keyframe(3.397f, -10.750f));
            temp.AddKey(new Keyframe(4.707f, -20.350f));
            temp.AddKey(new Keyframe(6.049f, -30.400f));
            temp.AddKey(new Keyframe(7.452f, -39.850f));
            temp.AddKey(new Keyframe(9.106f, -49.900f));
            temp.AddKey(new Keyframe(10.82f, -54.550f));
            temp.AddKey(new Keyframe(12.94f, -58.150f));
            temp.AddKey(new Keyframe(15.12f, -59.500f));



            // ---------------------------------- Density
            density = new AnimationCurve();
            density.AddKey(new Keyframe(0.028f, 1.241f));
            density.AddKey(new Keyframe(1.152f, 1.110f));
            density.AddKey(new Keyframe(1.939f, 1.019f));
            density.AddKey(new Keyframe(2.838f, 0.927f));
            density.AddKey(new Keyframe(3.709f, 0.841f));
            density.AddKey(new Keyframe(4.580f, 0.766f));
            density.AddKey(new Keyframe(5.620f, 0.680f));
            density.AddKey(new Keyframe(6.688f, 0.604f));
            density.AddKey(new Keyframe(7.924f, 0.524f));
            density.AddKey(new Keyframe(9.217f, 0.452f));
            density.AddKey(new Keyframe(10.369f, 0.385f));
            density.AddKey(new Keyframe(11.661f, 0.321f));
            density.AddKey(new Keyframe(13.094f, 0.261f));
            density.AddKey(new Keyframe(14.302f, 0.220f));
            density.AddKey(new Keyframe(15.623f, 0.181f));
            density.AddKey(new Keyframe(17.000f, 0.146f));
        }


        /// <summary>
        /// Calculates the deviaton of the aircraft from the waypoint direction
        /// </summary>
        /// <param name="aircraft"> Aircraft body transform</param>
        /// <param name="nextPoint"> Waypoint aircraft is moing to.</param>
        /// <param name="lastPoint"> Waypoint aircraft is coming from.</param>
        /// <returns>
        /// Float value of the deviation of the aircraft from the route in meters
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float CalculateRouteDeviation(Transform aircraft, Vector3 nextPoint, Vector3 lastPoint, int state, float anticipation)
        {
            float deviation, steerDirection;
            Vector3 currentPosition = aircraft.position + aircraft.forward * anticipation;


            // ------------------------------------------------------ Calculate Deviation
            Vector3 a = currentPosition - lastPoint;
            Vector3 b = nextPoint - lastPoint;
            float progress = (a.x * b.x + a.y * b.y + a.z * b.z) / (b.x * b.x + b.y * b.y + b.z * b.z);
            Vector3 errorPos = lastPoint + progress * b;
            if(state == 1) { errorPos.y = 0f; currentPosition.y = 0; }
            else { errorPos.x = 0;errorPos.z = 0f; currentPosition.x = 0f;currentPosition.z = 0; }
            deviation = (errorPos - currentPosition).magnitude;

            // ------------------------------------------------------ Determine if is left or right side
            Vector3 steerPosition = aircraft.position + aircraft.forward;
            Vector3 pointDirection = nextPoint - steerPosition;
            float dotProduct;
            if (state == 1) { dotProduct = Vector3.Dot(aircraft.right, pointDirection); }
            else { dotProduct = Vector3.Dot(aircraft.up, pointDirection); }  

            if (dotProduct > 0f) { steerDirection = 1f; }
            else { steerDirection = -1f; }
            deviation *= steerDirection;

            return deviation;
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float AddValueToAverage(float oldAverage, float valueToAdd, float count)
        {
            float newAverage = ((oldAverage * count) + valueToAdd) / (count + 1f);
            return newAverage;
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void ConfigurePathHeight(List<Vector3> points, float baseHeight, float finalHeight)
        {
            float BA = baseHeight /toFt;
            float FA = finalHeight /toFt;
            float heightFactor = (FA - BA);

            for (int i = 0; i < points.Count; i++)
            {
                if (points[i] != null)
                {
                    Vector3 point = new Vector3(points[i].x, BA + heightFactor * i / (points.Count), points[i].z);
                    points[i] = point;
                }
            }
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector3 BezierPathCalculation(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float tt = t * t;
            float ttt = t * tt;
            float u = 1.0f - t;
            float uu = u * u;
            float uuu = u * uu;

            Vector3 B = new Vector3();
            B = uuu * p0;
            B += 3.0f * uu * t * p1;
            B += 3.0f * u * tt * p2;
            B += ttt * p3;

            return B;
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static int ClampListIndex(int index, int listSize)
        {
            index = ((index % listSize) + listSize) % listSize;
            return index;
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
        {
            return 0.5f * ((2 * p1) + (-p0 + p2) * i + (2 * p0 - 5 * p1 + 4 * p2 - p3) * i * i + (-p0 + 3 * p1 - 3 * p2 + p3) * i * i * i);
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Interpolate between the supplied gain values based on the set factor
        /// </summary>
        /// <param name="lowerBoundGains">The gain set(vector) on the left/lower side of the desired gain point.</param>
        /// <param name="upperBoundGains">The gain set(vector) on the right/upper side of the desired gain point.</param>
        /// <param name="factor">Variable determinig how and when the interpolation is done.</param>
        /// <returns>
        /// gain Vector
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static GainVector AnalyseGain(Gain lowerBoundGains, Gain upperBoundGains, float factor)
        {
            GainVector gain = new GainVector
            {
                Kp = InterpolateValues(factor, upperBoundGains.Kp, lowerBoundGains.Kp, upperBoundGains.factor, lowerBoundGains.factor),
                Ki = InterpolateValues(factor, upperBoundGains.Ki, lowerBoundGains.Ki, upperBoundGains.factor, lowerBoundGains.factor),
                Kd = InterpolateValues(factor, upperBoundGains.Kd, lowerBoundGains.Kd, upperBoundGains.factor, lowerBoundGains.factor)
            };
            return gain;
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static AnimationCurve PlotControlEffectiveness()
        {
            AnimationCurve ƞ = new AnimationCurve();
            ƞ.AddKey(new Keyframe(0.0f, 0.8f));
            ƞ.AddKey(new Keyframe(9.862f, 0.803f));
            ƞ.AddKey(new Keyframe(14.315f, 0.769f));
            ƞ.AddKey(new Keyframe(18.244f, 0.717f));
            ƞ.AddKey(new Keyframe(20.940f, 0.652f));
            ƞ.AddKey(new Keyframe(24.246f, 0.584f));
            ƞ.AddKey(new Keyframe(29.343f, 0.527f));
            ƞ.AddKey(new Keyframe(34.509f, 0.485f));
            ƞ.AddKey(new Keyframe(40.916f, 0.446f));
            ƞ.AddKey(new Keyframe(46.608f, 0.418f));
            ƞ.AddKey(new Keyframe(54.252f, 0.385f));
            ƞ.AddKey(new Keyframe(60.650f, 0.364f));
            ƞ.AddKey(new Keyframe(69.710f, 0.338f));
            return ƞ;
        }






        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static AnimationCurve PlotControlInputCurve(float power)
        {
            if (power <= 0) { power = 1; }
            AnimationCurve inputCurve = new AnimationCurve();

            inputCurve.AddKey(new Keyframe(-1.0f, -Mathf.Pow(1.0f, power)));
            inputCurve.AddKey(new Keyframe(-0.9f, -Mathf.Pow(0.9f, power)));
            inputCurve.AddKey(new Keyframe(-0.8f, -Mathf.Pow(0.8f, power)));
            inputCurve.AddKey(new Keyframe(-0.7f, -Mathf.Pow(0.7f, power)));
            inputCurve.AddKey(new Keyframe(-0.6f, -Mathf.Pow(0.6f, power)));
            inputCurve.AddKey(new Keyframe(-0.5f, -Mathf.Pow(0.5f, power)));
            inputCurve.AddKey(new Keyframe(-0.4f, -Mathf.Pow(0.4f, power)));
            inputCurve.AddKey(new Keyframe(-0.3f, -Mathf.Pow(0.3f, power)));
            inputCurve.AddKey(new Keyframe(-0.2f, -Mathf.Pow(0.2f, power)));
            inputCurve.AddKey(new Keyframe(-0.1f, -Mathf.Pow(0.1f, power)));

            inputCurve.AddKey(new Keyframe(0.0f, Mathf.Pow(0.0f, power)));

            inputCurve.AddKey(new Keyframe(0.1f, Mathf.Pow(0.1f, power)));
            inputCurve.AddKey(new Keyframe(0.2f, Mathf.Pow(0.2f, power)));
            inputCurve.AddKey(new Keyframe(0.3f, Mathf.Pow(0.3f, power)));
            inputCurve.AddKey(new Keyframe(0.4f, Mathf.Pow(0.4f, power)));
            inputCurve.AddKey(new Keyframe(0.5f, Mathf.Pow(0.5f, power)));
            inputCurve.AddKey(new Keyframe(0.6f, Mathf.Pow(0.6f, power)));
            inputCurve.AddKey(new Keyframe(0.7f, Mathf.Pow(0.7f, power)));
            inputCurve.AddKey(new Keyframe(0.8f, Mathf.Pow(0.8f, power)));
            inputCurve.AddKey(new Keyframe(0.9f, Mathf.Pow(0.9f, power)));
            inputCurve.AddKey(new Keyframe(1.0f, Mathf.Pow(1.0f, power)));

#if UNITY_EDITOR
            for (int i = 0; i < inputCurve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(inputCurve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(inputCurve, i, AnimationUtility.TangentMode.Auto);
            }
#endif

            return inputCurve;
        }





        // Rounds a float variable to a given set of digits
        /// <summary>
        /// Rounds a float variable to a given set of digits
        /// </summary>
        /// <param name="value">Variable to round.</param>
        /// <param name="digits">Number of digits to round to.</param>
        /// <returns>
        /// The rounded float
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float Round(float value, int digits)
        {
            float mult = Mathf.Pow(10.0f, (float)digits);
            return Mathf.Round(value * mult) / mult;
        }






        // Converts a given vector from the earth axis to the body axis
        /// <summary>
        /// Converts a given vector from the earth axis to the body axis
        /// </summary>
        /// <param name="earthVelocity">Vector to convert.</param>
        /// <param name="body"> Tranform to use for the conversion.</param>
        /// <returns>
        /// The given input vector in body axis
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector3 TransformVelocityToBodyAxis(Vector3 earthVelocity, Transform body)
        {
            Vector3 bodyVelocity;
            Vector3 rot = body.transform.eulerAngles;

            float x = earthVelocity.z;
            float y = earthVelocity.x;
            float z = earthVelocity.y;

            float θ = -rot.x * Mathf.Deg2Rad;
            float ψ = rot.y * Mathf.Deg2Rad;
            float φ = -rot.z * Mathf.Deg2Rad;

            float x1 = Mathf.Cos(θ) * Mathf.Cos(ψ);
            float x2 = (Mathf.Sin(φ) * Mathf.Sin(θ) * Mathf.Cos(ψ)) - (Mathf.Cos(φ) * Mathf.Sin(ψ));
            float x3 = (Mathf.Cos(φ) * Mathf.Sin(θ) * Mathf.Cos(ψ)) + (Mathf.Sin(φ) * Mathf.Sin(ψ));

            float y1 = Mathf.Cos(θ) * Mathf.Sin(ψ);
            float y2 = (Mathf.Sin(φ) * Mathf.Sin(θ) * Mathf.Sin(ψ)) + (Mathf.Cos(φ) * Mathf.Cos(ψ));
            float y3 = (Mathf.Cos(φ) * Mathf.Sin(θ) * Mathf.Sin(ψ)) - (Mathf.Sin(φ) * Mathf.Cos(ψ));

            float z1 = -Mathf.Sin(θ);
            float z2 = Mathf.Sin(φ) * Mathf.Cos(θ);
            float z3 = Mathf.Cos(φ) * Mathf.Cos(θ);

            float U = (x1 * x) + (y1 * y) + (z1 * z);
            float V = (x2 * x) + (y2 * y) + (z2 * z);
            float W = (x3 * x) + (y3 * y) + (z3 * z);

            bodyVelocity = new Vector3(U, V, W);
            return bodyVelocity;
        }




        /// <summary>
        /// Converts speed in m/s to other speed units
        /// </summary>
        /// <param name="inputSpeed">Input speed in m/s.</param>
        /// <param name="conversionID">3 letter ID representing output unit e.g KTS, KPH, MPH e.t.c.</param>
        /// <returns>
        /// Converted Speed.
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float ConvertSpeed(float inputSpeed, string conversionID)
        {
            float baseSpeed = 0f;

            if (conversionID == "KTS") { baseSpeed = inputSpeed * 1.94384f; }
            if (conversionID == "KPH") { baseSpeed = inputSpeed * 3.6f; }
            if (conversionID == "MPH") { baseSpeed = inputSpeed * 2.2369311202577f; }
            if (conversionID == "KTS") { baseSpeed = inputSpeed * 1.94384f; }

            return baseSpeed;
        }










        /// <summary>
        /// Conversion variables
        /// </summary>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <value>Gets the horizontal speed conversion float from m/s to knots.</value>
        public const float toKnots = 1.94384F;
        /// <value>Gets the conversion float from m to ft.</value>
        public const float toFt = 3.28084F;
        /// <value>Gets the vertical speed conversion float from m/s to ft/min.</value>
        public const float toFtMin = 196.8504F;











        /// <summary>
        /// Converts distance in m to other distance units
        /// </summary>
        /// <param name="inputDistance">Input distance in m.</param>
        /// <param name="conversionID">2 letter ID representing output unit e.g KT, FT, ML e.t.c.</param>
        /// <returns>
        /// Converted distance float
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float ConvertDistance(float inputDistance, string conversionID)
        {
            float baseDistance = 0f;

            if (conversionID == "KM") { baseDistance = inputDistance * 0.001f; }
            if (conversionID == "FT") { baseDistance = inputDistance * 3.28084f; }
            if (conversionID == "ML") { baseDistance = inputDistance * 0.000621371f; }
            if (conversionID == "NM") { baseDistance = inputDistance * 0.000539957f; }

            return baseDistance;
        }







        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void DrawCorrectionCurves(out AnimationCurve powerCorrection, out AnimationCurve swirlCorrection)
        {
            swirlCorrection = new AnimationCurve();
            powerCorrection = new AnimationCurve();
            //-----------------------------------SWIRL
            Keyframe a1 = new Keyframe(0.005f, 0.013f); Keyframe b1 = new Keyframe(0.010f, 0.025f); Keyframe c1 = new Keyframe(0.015f, 0.037f);
            Keyframe d1 = new Keyframe(0.020f, 0.049f); Keyframe e1 = new Keyframe(0.025f, 0.057f); Keyframe f1 = new Keyframe(0.030f, 0.063f); Keyframe g1 = new Keyframe(0.035f, 0.075f);
            Keyframe h1 = new Keyframe(0.040f, 0.086f); Keyframe i1 = new Keyframe(0.045f, 0.093f); Keyframe j1 = new Keyframe(0.050f, 0.11f);
            //PLOT
            swirlCorrection.AddKey(a1); swirlCorrection.AddKey(b1); swirlCorrection.AddKey(c1); swirlCorrection.AddKey(d1); swirlCorrection.AddKey(e1); swirlCorrection.AddKey(f1);
            swirlCorrection.AddKey(g1); swirlCorrection.AddKey(h1); swirlCorrection.AddKey(i1); swirlCorrection.AddKey(j1);

            //----------------------------------POWER
            Keyframe a2 = new Keyframe(0.3f, 1.010f); Keyframe b2 = new Keyframe(0.4f, 1.022f); Keyframe c2 = new Keyframe(0.5f, 1.040f); Keyframe d2 = new Keyframe(0.6f, 1.043f); Keyframe e2 = new Keyframe(0.7f, 1.058f);
            Keyframe f2 = new Keyframe(0.8f, 1.061f); Keyframe g2 = new Keyframe(0.9f, 1.064f); Keyframe h2 = new Keyframe(1.0f, 1.068f); Keyframe i2 = new Keyframe(1.1f, 1.073f); Keyframe j2 = new Keyframe(1.2f, 1.080f);
            //PLOT
            powerCorrection.AddKey(a2); powerCorrection.AddKey(b2); powerCorrection.AddKey(c2); powerCorrection.AddKey(d2); powerCorrection.AddKey(e2);
            powerCorrection.AddKey(f2); powerCorrection.AddKey(g2); powerCorrection.AddKey(h2); powerCorrection.AddKey(i2); powerCorrection.AddKey(j2);
        }






        /// <summary>
        /// Calculates the variation in Lift and Drag for a given aerofoil based on its interaction with a set ground surface
        /// </summary>
        /// <param name="foil">Input distance in m.</param>
        /// <param name="axis"> Direction to shoot the raycast for calculating ground distance.</param>
        /// <param name="center"> Vector point to start the calculation from.</param>
        /// <param name="span"> Horizontal span of the given aerofoil</param>
        /// <param name="layer">Ground layers to consider.</param>
        /// <returns>
        /// Float describing the lift increase and drag decrease
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float EstimateGroundEffectFactor(Transform foil, Vector3 axis, Vector3 center, float span, LayerMask layer)
        {
            float groundFactor = 1f;
            Vector3 baseDirection = foil.rotation * axis;
            Ray groundCheck = new Ray(center, baseDirection);
            RaycastHit groundHit;
            if (Physics.Raycast(groundCheck, out groundHit, span, layer))
            {
                // ------------------------- Collect Factor
                float spanRatio = groundHit.distance / span;
                float GA = 1 + (3.142f / (8f * spanRatio));
                float GB = (2 / (3.142f * 3.142f)) * Mathf.Log(GA);
                groundFactor = 1 - GB;
            }

            return groundFactor;
        }





        /// <summary>
        /// Calculates the mean chord for a given aerofoil or aerofoil panel
        /// </summary>
        /// <param name="setionRootChord">Foil or Panel root chord.</param>
        /// <param name="sectionTipChord">Foil or Panel tip chord.</param>
        /// <returns>
        /// Calculated mean chord in m
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float EstimateMeanChord(float setionRootChord, float sectionTipChord)
        {
            float taperRatio = sectionTipChord / setionRootChord;
            float lamdaChord = (1 + taperRatio + (taperRatio * taperRatio)) / (1 + taperRatio);
            float sectionMeanChord = 0.66667f * setionRootChord * lamdaChord;
            return sectionMeanChord;
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculates the surface area of a given aerofoil panel
        /// </summary>
        /// <param name="span">Panel span.</param>
        /// <param name="rootChord">Panel root chord.</param>
        /// <param name="tipChord">Panel tip chord.</param>
        /// <returns>
        /// Calculated surface area in m2
        /// </returns>
        public static float EstimatePanelArea(float span, float rootChord, float tipChord) { float area = 0.5f * span * (rootChord + tipChord); return area; }
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculates vector point at a given distance along an aerofoil section line
        /// </summary>
        /// <param name="lhs">Left vector boundary.</param>
        /// <param name="rhs">Right vector boundary.</param>
        /// <param name="factor"> Distance along aerofoil line.</param>
        /// <returns>
        /// Calculated vector3 point
        /// </returns>
        public static Vector3 EstimateSectionPosition(Vector3 lhs, Vector3 rhs, float factor) { Vector3 estimatedPosition = lhs + ((rhs - lhs) * factor); return estimatedPosition; }
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Interpolates a given value at a particular distance given the root and tip variables e.g lift coefficent at a given division point
        /// </summary>
        /// <param name="rootValue">Left vector boundary.</param>
        /// <param name="tipValue">Right vector boundary.</param>
        /// <param name="yM"> Distance of panel trailing edge from the aerofoil root trailing edge.</param>
        /// <param name="wingTip"> Wingtip extension amount.</param>
        /// <param name="foilSpan"> Distance along aerofoil line.</param>
        /// <returns>
        /// Calculated point value
        /// </returns>
        public static float EstimateEffectiveValue(float rootValue, float tipValue, float yM, float wingTip, float foilSpan) { float baseValue = rootValue + ((yM / (foilSpan + wingTip)) * (tipValue - rootValue)); return baseValue; }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Twists the line between two vector points by a given amount
        /// </summary>
        /// <param name="deflection">Twist amount in degrees.</param>
        /// <param name="alphaPoint"> Leading chord point.</param>
        /// <param name="betaPoint"> Trailing chord point.</param>
        /// <param name="obj"Parent transform.</param>
        /// <returns>
        /// Calculated point value
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector3 EstimateSkewDistance(float deflection, Vector3 alphaPoint, Vector3 betaPoint, Transform obj)
        {
            Vector3 skewDistance = (Quaternion.AngleAxis(deflection, obj.right.normalized) * (alphaPoint - betaPoint)) * 0.5f;
            return skewDistance;
        }





        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void PlotSpoilerData(out AnimationCurve spoilerDragCurve)
        {
            spoilerDragCurve = new AnimationCurve(); spoilerDragCurve.AddKey(new Keyframe(1, 1.15f));
            spoilerDragCurve.AddKey(new Keyframe(2, 1.16f)); spoilerDragCurve.AddKey(new Keyframe(5, 1.20f));
            spoilerDragCurve.AddKey(new Keyframe(10, 1.22f)); spoilerDragCurve.AddKey(new Keyframe(30, 1.62f));
        }





        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void EstimateFoilShapeProperties(string twistDirection, string sweepDirection, float inputTwist, float inputSweep, out float outputTwist, out float outputSweep)
        {
            outputSweep = 0f;
            outputTwist = 0f;

            if (sweepDirection == "Unswept") { outputSweep = 0; }
            else if (sweepDirection == "Forward") { outputSweep = inputSweep; }
            else if (sweepDirection == "Backward") { outputSweep = -inputSweep; }

            if (twistDirection == "Untwisted") { outputTwist = 0; }
            else if (twistDirection == "Downwards") { outputTwist = inputTwist; }
            else if (twistDirection == "Upwards") { outputTwist = -inputTwist; }
        }





        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Linearly interpolates between a given set of variables using the formula
        /// x = x0 + [(y-y0)(x1-x0)/(y1-y0)]
        /// </summary>
        /// <param name="Vx"> y in the given equation.</param>
        /// <param name="C1"> x1 in the given equation.</param>
        /// <param name="C0"> x0 in the given equation.</param>
        /// <param name="V1"> y1 in the given equation.</param>
        /// <param name="V0"> y0 in the given equation.</param>
        /// <returns>
        /// Calculated point value
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float InterpolateValues(float Vx, float C1, float C0, float V1, float V0)
        {
            float A, B, C, D;

            A = C1 - C0;
            B = Vx - V0;
            C = V1 - V0;
            D = C0;

            float value = ((A * B) / C) + D;
            //RETURN
            return value;
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void AnalysePolar(string dataPath, out List<float> lift, out List<float> drag, out List<float> moment, char lineSeperator, char fieldSeperator)
        {
            lift = new List<float>();
            drag = new List<float>();
            moment = new List<float>();

            StreamReader shape = new StreamReader(dataPath);
            string shapeText = shape.ReadToEnd();
            string[] superPlots = shapeText.Split(lineSeperator);
            for (int j = 5; (j < superPlots.Length - 3); j++)
            {
                string[] data = superPlots[j].Split(fieldSeperator);
                float cl = float.Parse(data[1]); float cd = float.Parse(data[2]); float cm = float.Parse(data[3]);
                //STORE LISTS
                lift.Add(cl);
                drag.Add(cd);
                moment.Add(cm);
            }
        }





        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculates the geometric center of a given aerofoil panel
        /// </summary>
        /// <param name="LeadEdgeRight"> </param>
        /// <param name="TrailEdgeRight"> </param>
        /// <param name="LeadEdgeLeft"> </param>
        /// <param name="TrailEdgeLeft"> </param>
        /// <returns>
        /// Calculated center vector
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector3 EstimateGeometricCenter(Vector3 LeadEdgeRight, Vector3 TrailEdgeRight, Vector3 LeadEdgeLeft, Vector3 TrailEdgeLeft)
        {
            Vector3 center, sectionRootCenter, sectionTipCenter, yMGCTop, yMGCBottom;
            float sectionTipChord, sectionRootChord, sectionTaper;

            //CALCULATE GEOMETRIC CENTER FACTOR
            sectionTipChord = Vector3.Distance(LeadEdgeRight, TrailEdgeRight);
            sectionRootChord = Vector3.Distance(LeadEdgeLeft, TrailEdgeLeft);
            sectionTaper = sectionTipChord / sectionRootChord;

            sectionRootCenter = EstimateSectionPosition(LeadEdgeLeft, TrailEdgeLeft, 0.5f);
            sectionTipCenter = EstimateSectionPosition(LeadEdgeRight, TrailEdgeRight, 0.5f);
            float sectionSpan = Vector3.Distance(sectionRootCenter, sectionTipCenter);
            float yM = (sectionSpan / 6) * ((1 + (2 * sectionTaper)) / (1 + sectionTaper));
            float factor = yM / sectionSpan;

            yMGCTop = EstimateSectionPosition(TrailEdgeLeft, LeadEdgeLeft, (1 - factor));
            yMGCBottom = EstimateSectionPosition(TrailEdgeRight, LeadEdgeRight, (1 - factor));
            center = EstimateSectionPosition(yMGCTop, yMGCBottom, 0.5f);

            return center;
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float EstimatePanelSectionArea(Vector3 panelLeadingLeft, Vector3 panelLeadingRight, Vector3 panelTrailingLeft, Vector3 panelTrailingRight)
        {
            //BUILD TRAPEZOID VARIABLES
            float panelArea, panelLeadingEdge, panelTipEdge, panalTrailingEdge, paneRootEdge, panelDiagonal;
            //SOLVE
            panelLeadingEdge = (panelTrailingLeft - panelLeadingLeft).magnitude; panelTipEdge = (panelTrailingRight - panelTrailingLeft).magnitude; panalTrailingEdge = (panelLeadingRight - panelTrailingRight).magnitude; paneRootEdge = (panelLeadingLeft - panelLeadingRight).magnitude;
            panelDiagonal = 0.5f * (panelLeadingEdge + panelTipEdge + panalTrailingEdge + paneRootEdge);
            panelArea = Mathf.Sqrt(((panelDiagonal - panelLeadingEdge) * (panelDiagonal - panelTipEdge) * (panelDiagonal - panalTrailingEdge) * (panelDiagonal - paneRootEdge)));
            return panelArea;
        }





        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculates the effectivenss of a control surface based on the speed and the control actuator torque
        /// </summary>
        /// <param name="inputSpeed"> Speed to evaluate the efficiency at</param>
        /// <param name="inputArea"> Area of the control surface</param>
        /// <param name="inputTorque"> Maximum torque of the surface actuator</param>
        /// <returns>
        /// Calculated efficiency
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float EstimateControlEfficiency(float inputSpeed, float inputArea, float inputTorque)
        {
            float factor = inputTorque / (inputSpeed * inputSpeed * inputArea);
            if (factor > 1) { factor = 1f; }
            if (factor < -1) { factor = -1f; }

            float efficiency = Mathf.Asin(factor) * Mathf.Rad2Deg;
            return efficiency;
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculates the speed that a control surface will reach  a given effectiveness value
        /// </summary>
        /// <param name="inputEfficiency"> efficiency to evaluate the speed at</param>
        /// <param name="inputArea"> Area of the control surface</param>
        /// <param name="inputTorque"> Maximum torque of the surface actuator</param>
        /// <returns>
        /// Calculated speed
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float EstimateEfficiencySpeed(float inputEfficiency, float inputArea, float inputTorque)
        {
            float A1 = Mathf.Sin(inputEfficiency / Mathf.Rad2Deg);
            float A2 = A1 * inputArea;
            float speed = Mathf.Sqrt(inputTorque / A2);
            return speed;
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void AnalyseEngineDimensions(float engineDiameter, float intakeP, float exhaustP, out float diffuserDiameter, out float exhaustDiameter)
        {
            // Calculate Diameters
            diffuserDiameter = engineDiameter * (intakeP / 100f);
            exhaustDiameter = engineDiameter * (exhaustP / 100f);
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static AnimationCurve DrawPressureFactor()
        {
            AnimationCurve pressureFactor = new AnimationCurve();
            //-----------------------------------SPECIFIC PRESSURE
            Keyframe a1 = new Keyframe(250, 1.003f); Keyframe b1 = new Keyframe(300, 1.005f); Keyframe c1 = new Keyframe(350, 1.008f);
            Keyframe d1 = new Keyframe(400, 1.013f); Keyframe e1 = new Keyframe(450, 1.020f); Keyframe f1 = new Keyframe(500, 1.029f);
            Keyframe g1 = new Keyframe(550, 1.040f); Keyframe h1 = new Keyframe(600, 1.051f); Keyframe i1 = new Keyframe(650, 1.063f);
            Keyframe j1 = new Keyframe(700, 1.075f); Keyframe k1 = new Keyframe(750, 1.087f); Keyframe l1 = new Keyframe(800, 1.099f); Keyframe m1 = new Keyframe(900, 1.121f);
            Keyframe n1 = new Keyframe(1000, 1.142f); Keyframe o1 = new Keyframe(1100, 1.155f); Keyframe p1 = new Keyframe(1200, 1.173f);
            Keyframe q1 = new Keyframe(1300, 1.190f); Keyframe r1 = new Keyframe(1400, 1.204f); Keyframe s1 = new Keyframe(1500, 1.216f);
            //PLOT
            pressureFactor.AddKey(a1); pressureFactor.AddKey(b1); pressureFactor.AddKey(c1); pressureFactor.AddKey(d1); pressureFactor.AddKey(e1); pressureFactor.AddKey(f1);
            pressureFactor.AddKey(g1); pressureFactor.AddKey(h1); pressureFactor.AddKey(i1); pressureFactor.AddKey(j1); pressureFactor.AddKey(k1); pressureFactor.AddKey(l1);
            pressureFactor.AddKey(m1); pressureFactor.AddKey(n1); pressureFactor.AddKey(o1); pressureFactor.AddKey(p1); pressureFactor.AddKey(q1); pressureFactor.AddKey(r1);
            pressureFactor.AddKey(s1);

            return pressureFactor;
        }





        public static AnimationCurve DrawAdiabaticConstant()
        {
            AnimationCurve adiabaticFactor = new AnimationCurve();
            //---------------------------------ADIABATIC CONSTANT
            Keyframe a1 = new Keyframe(250, 1.401f); Keyframe b1 = new Keyframe(300, 1.400f); Keyframe c1 = new Keyframe(350, 1.398f); Keyframe d1 = new Keyframe(400, 1.395f);
            Keyframe e1 = new Keyframe(450, 1.391f); Keyframe f1 = new Keyframe(500, 1.387f); Keyframe g1 = new Keyframe(550, 1.381f); Keyframe h1 = new Keyframe(600, 1.376f);
            Keyframe i1 = new Keyframe(650, 1.370f); Keyframe j1 = new Keyframe(700, 1.364f); Keyframe k1 = new Keyframe(750, 1.359f); Keyframe l1 = new Keyframe(800, 1.354f);
            Keyframe m1 = new Keyframe(900, 1.344f); Keyframe n1 = new Keyframe(1000, 1.336f); Keyframe o1 = new Keyframe(1100, 1.331f); Keyframe p1 = new Keyframe(1200, 1.324f);
            Keyframe q1 = new Keyframe(1300, 1.318f); Keyframe r1 = new Keyframe(1400, 1.313f); Keyframe s1 = new Keyframe(1500, 1.309f);
            //PLOT
            adiabaticFactor.AddKey(a1); adiabaticFactor.AddKey(b1); adiabaticFactor.AddKey(c1); adiabaticFactor.AddKey(d1); adiabaticFactor.AddKey(e1); adiabaticFactor.AddKey(f1); adiabaticFactor.AddKey(g1); adiabaticFactor.AddKey(h1);
            adiabaticFactor.AddKey(i1); adiabaticFactor.AddKey(j1); adiabaticFactor.AddKey(k1); adiabaticFactor.AddKey(l1); adiabaticFactor.AddKey(m1); adiabaticFactor.AddKey(n1); adiabaticFactor.AddKey(o1); adiabaticFactor.AddKey(p1);
            adiabaticFactor.AddKey(q1); adiabaticFactor.AddKey(r1); adiabaticFactor.AddKey(s1);

            return adiabaticFactor;
        }





        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculates the reynolds number for a given speed and variable set
        /// </summary>
        /// <param name="inputSpeed"> efficiency to evaluate the speed at</param>
        /// <param name="charLength"> characteristic length of the object interacting with the fluid</param>
        /// <param name="airDensity"> </param>
        /// <param name="viscocity"> </param>
        /// <param name="machSpeed"> </param>
        /// <param name="k"> Surface roughness factor</param>
        /// <returns>
        /// Calculated reynolds number
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float EstimateRe(float inputSpeed, float charLength, float airDensity, float viscocity, float machSpeed, float k)
        {
            float Re1 = (airDensity * inputSpeed * charLength) / viscocity; float Re2;
            if (machSpeed < 0.9f) { Re2 = 38.21f * Mathf.Pow(((charLength * 3.28f) / (k / 100000)), 1.053f); }
            else { Re2 = 44.62f * Mathf.Pow(((charLength * 3.28f) / (k / 100000)), 1.053f) * Mathf.Pow(machSpeed, 1.16f); }
            return Mathf.Min(Re1, Re2);
        }






        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float EstimateSkinDragCoefficient(float Cr, float Ct, float velocity, float airDensity, float viscocity, float machSpeed, float k)
        {
            float Recr = EstimateRe(velocity, Cr, airDensity, viscocity, machSpeed, k); float Rect = EstimateRe(velocity, Ct, airDensity, viscocity, machSpeed, k);
            float X0Cr1 = 36.9f * 0.366f * Mathf.Pow((1 / Recr), 0.375f);//Root Upper
            float X0Ct1 = 36.9f * 0.366f * Mathf.Pow((1 / Rect), 0.375f);//Tip Upper
            float X0Ct2 = 36.9f * 0.237f * Mathf.Pow((1 / Rect), 0.375f);//Tip Upper

            float Cf1 = (0.074f / Mathf.Pow(Recr, 0.2f)) * Mathf.Pow((1 - (0.2f - X0Cr1)), 0.8f);
            float Cfu2 = (0.074f / Mathf.Pow(Rect, 0.2f)) * Mathf.Pow((1 - (0.2f - X0Ct1)), 0.8f);
            float Cfl2 = (0.074f / Mathf.Pow(Rect, 0.2f)) * Mathf.Pow((1 - (0.1f - X0Ct2)), 0.8f);//X0Ct1 TO Reduce...Check Later
            float Cf2 = (Cfu2 + Cfl2) / 2f;
            return (Cf1 + Cf2) / 2f;
        }





        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void CartesianToSpherical(Vector3 cordinatesInput, out float outRadius, out float outPolar, out float outElevation)
        {
            if (cordinatesInput.x == 0) cordinatesInput.x = Mathf.Epsilon;
            outRadius = Mathf.Sqrt((cordinatesInput.x * cordinatesInput.x) + (cordinatesInput.y * cordinatesInput.y) + (cordinatesInput.z * cordinatesInput.z));
            outPolar = Mathf.Atan(cordinatesInput.z / cordinatesInput.x);
            if (cordinatesInput.x < 0) outPolar += Mathf.PI;
            outElevation = Mathf.Asin(cordinatesInput.y / outRadius);
        }
        static float jester;
        public static void SphericalToCartesian(float radiusInput, float polarInput, float elevationInput, out Vector3 position)
        {
            jester = radiusInput * Mathf.Cos(elevationInput); position.x = jester * Mathf.Cos(polarInput);
            position.y = radiusInput * Mathf.Sin(elevationInput); position.z = jester * Mathf.Sin(polarInput);
        }







        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculates the drag generated by a control surface as a result of its deflection
        /// </summary>
        /// <param name="Rf"> Control surface chors</param>
        /// <param name="deflection"> absolute deflection of the control surface in degrees</param>
        /// <param name="factor"> Control surface type: 1 = plain flap, 2 = split flap, 3 = slotted flap</param>
        /// <param name="area"> Control surface area</param>
        /// <param name="foilArea"> Total aerofoil area</param>
        /// <returns>
        /// Calculated drag Coefficient
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float EstimateDeflectionDrag(float Rf, float deflection, int factor, float area, float foilArea)
        {
            Rf /= 100f;
            float factor1 = 0f, factor2 = 0f;
            //PLAIN
            if (factor == 1)
            {
                factor1 = (4.6945f * Rf * Rf) + (4.3721f * Rf) - 0.031f;
                factor2 = ((-3.795f / 10000000) * Mathf.Pow(deflection, 3)) +
                    ((5.387f / 100000) * Mathf.Pow(deflection, 2)) +
                    ((6.843f / 10000) * deflection) - (1.4729f / 1000);
            }
            //SPLIT
            if (factor == 2)
            {
                factor1 = (4.6945f * Rf * Rf) + (4.3721f * Rf) - 0.031f;
                factor2 = ((-3.2740f / 10000000) * Mathf.Pow(deflection, 3)) +
                    ((5.598f / 100000) * Mathf.Pow(deflection, 2)) -
                    ((1.2443f / 10000) * deflection) + (5.1647f / 10000);
            }
            //SLOTTED
            if (factor == 3)
            {
                factor1 = (8.2658f * Rf * Rf) + (3.4564f * Rf) + 0.0054f;
                factor2 = ((-3.681f / 10000000) * Mathf.Pow(deflection, 3)) +
                    ((5.3342f / 100000) * Mathf.Pow(deflection, 2)) -
                    ((4.1677f / 1000) * deflection) + (6.749f / 10000);
            }
            //RETURN
            float dragCoefficient = factor1 * factor2 * (area / foilArea);
            return dragCoefficient;
        }






        #region Dubin Math


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector3 GetRightCircleCenterPos(Vector3 carPos, float heading, float turnRadius)
        {
            Vector3 rightCirclePos = Vector3.zero;
            rightCirclePos.x = carPos.x + turnRadius * Mathf.Sin(heading + (Mathf.PI / 2f));
            rightCirclePos.z = carPos.z + turnRadius * Mathf.Cos(heading + (Mathf.PI / 2f));
            return rightCirclePos;
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector3 GetLeftCircleCenterPos(Vector3 carPos, float heading, float turnRadius)
        {
            Vector3 rightCirclePos = Vector3.zero;
            rightCirclePos.x = carPos.x + turnRadius * Mathf.Sin(heading - (Mathf.PI / 2f));
            rightCirclePos.z = carPos.z + turnRadius * Mathf.Cos(heading - (Mathf.PI / 2f));
            return rightCirclePos;
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void LSLorRSR(Vector3 startCircle, Vector3 goalCircle, bool isBottom, out Vector3 baseTangent, out Vector3 finalTangent, float turnRadius)
        {
            float theta = 90f * Mathf.Deg2Rad;
            theta += Mathf.Atan2(goalCircle.z - startCircle.z, goalCircle.x - startCircle.x);
            if (isBottom) { theta += Mathf.PI; }
            float xT1 = startCircle.x + turnRadius * Mathf.Cos(theta);
            float zT1 = startCircle.z + turnRadius * Mathf.Sin(theta);

            Vector3 dirVec = goalCircle - startCircle;
            float xT2 = xT1 + dirVec.x; float zT2 = zT1 + dirVec.z;
            baseTangent = new Vector3(xT1, 0f, zT1);
            finalTangent = new Vector3(xT2, 0f, zT2);
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void RSLorLSR(Vector3 startCircle, Vector3 goalCircle, bool isBottom, out Vector3 baseTangent, out Vector3 finalTangent, float turnRadius)
        {
            float D = (startCircle - goalCircle).magnitude;
            float theta = Mathf.Acos((2f * turnRadius) / D);
            if (isBottom) { theta *= -1f; }
            theta += Mathf.Atan2(goalCircle.z - startCircle.z, goalCircle.x - startCircle.x);

            //The coordinates of the first tangent point
            float xT1 = startCircle.x + turnRadius * Mathf.Cos(theta);
            float zT1 = startCircle.z + turnRadius * Mathf.Sin(theta);
            float xT1_tmp = startCircle.x + 2f * turnRadius * Mathf.Cos(theta);
            float zT1_tmp = startCircle.z + 2f * turnRadius * Mathf.Sin(theta);
            Vector3 dirVec = goalCircle - new Vector3(xT1_tmp, 0f, zT1_tmp);

            float xT2 = xT1 + dirVec.x;
            float zT2 = zT1 + dirVec.z;

            baseTangent = new Vector3(xT1, 0f, zT1);
            finalTangent = new Vector3(xT2, 0f, zT2);
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void GetRLRorLRLTangents(Vector3 startCircle, Vector3 goalCircle, bool isLRL, out Vector3 baseTangent, out Vector3 finalTangent, out Vector3 middleCircle, float turnRadius)
        {
            //The distance between the circles
            float D = (startCircle - goalCircle).magnitude;
            float theta = Mathf.Acos(D / (4f * turnRadius));
            Vector3 V1 = goalCircle - startCircle;
            if (isLRL) { theta = Mathf.Atan2(V1.z, V1.x) + theta; }
            else { theta = Mathf.Atan2(V1.z, V1.x) - theta; }

            //Calculate the position of the third circle
            float x = startCircle.x + 2f * turnRadius * Mathf.Cos(theta);
            float y = startCircle.y;
            float z = startCircle.z + 2f * turnRadius * Mathf.Sin(theta);
            middleCircle = new Vector3(x, y, z);

            //Calculate the tangent points
            Vector3 V2 = (startCircle - middleCircle).normalized;
            Vector3 V3 = (goalCircle - middleCircle).normalized;
            baseTangent = middleCircle + V2 * turnRadius;
            finalTangent = middleCircle + V3 * turnRadius;
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float GetArcLength(Vector3 circleCenterPos, Vector3 basePosition, Vector3 finalPosition, bool isLeftCircle, float turnRadius)
        {
            Vector3 V1 = basePosition - circleCenterPos;
            Vector3 V2 = finalPosition - circleCenterPos;

            float theta = Mathf.Atan2(V2.z, V2.x) - Mathf.Atan2(V1.z, V1.x);
            if (theta < 0f && isLeftCircle) { theta += 2f * Mathf.PI; }
            else if (theta > 0 && !isLeftCircle) { theta -= 2f * Mathf.PI; }
            float arcLength = Mathf.Abs(theta * turnRadius);
            return arcLength;
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void AddCoordinatesToPath(ref Vector3 currentPos, ref float theta, List<Vector3> finalPath, int segments, bool isTurning, bool isTurningRight, float turnRadius, float stepDistance)
        {
            for (int i = 0; i < segments; i++)
            {
                currentPos.x += stepDistance * Mathf.Sin(theta);
                currentPos.z += stepDistance * Mathf.Cos(theta);

                if (isTurning)
                {
                    float turnParameter = 1f;
                    if (!isTurningRight) { turnParameter = -1f; }
                    theta += (stepDistance / turnRadius) * turnParameter;
                }
                //Add the new coordinate to the path
                finalPath.Add(currentPos);
            }
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static List<Vector3> DivideConnection(Vector3 from, Vector3 to, int chunkAmount)
        {
            List<Vector3> result = new List<Vector3>();
            float divider = 1f / chunkAmount;
            float linear = 0f;

            if (chunkAmount == 0)
            {
                Debug.LogError("chunkAmount Distance must be > 0 instead of " + chunkAmount);
                return null;
            }
            if (chunkAmount == 1)
            {
                result.Add(Vector3.Lerp(from, to, 0.5f));
                return result;
            }

            for (int i = 0; i < chunkAmount; i++)
            {
                if (i == 0) { linear = divider / 2; }
                else { linear += divider; }
                result.Add(Vector3.Lerp(from, to, linear));
            }

            return result;
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector3 GetWaypointPos(SilantroPath path, int index)
        {
            int waypointIndex = ClampListIndex(index, path.pathVectorPoints.Count);
            Vector3 waypointPos = path.pathVectorPoints[waypointIndex];
            return waypointPos;
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector3 GetWaypointPosition(List<Vector3> vectors, int index)
        {
            int waypointIndex = ClampListIndex(index, vectors.Count);
            Vector3 waypointPos = vectors[waypointIndex];
            return waypointPos;
        }





        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float CalculateProgress(Vector3 referencePoint, Vector3 wp_from, Vector3 wp_to)
        {
            float Rx = referencePoint.x - wp_from.x;
            float Rz = referencePoint.z - wp_from.z;

            float deltaX = wp_to.x - wp_from.x;
            float deltaZ = wp_to.z - wp_from.z;
            float progress = ((Rx * deltaX) + (Rz * deltaZ)) / ((deltaX * deltaX) + (deltaZ * deltaZ));
            return progress;
        }


        #endregion Dubin Math



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector2 GetClosestPointOnLine(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 a_p = p - a;
            Vector2 a_b = b - a;

            float sqrMagnitudeAB = a_b.sqrMagnitude;
            float ABAPproduct = Vector2.Dot(a_p, a_b);
            float distance = ABAPproduct / sqrMagnitudeAB;
            return a + a_b * distance;
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Interpolate between the supplied gain values based on the set factor
        /// </summary>
        /// <param name="lowerBoundGains">The gain set(vector) on the left/lower side of the desired gain point.</param>
        /// <param name="upperBoundGains">The gain set(vector) on the right/upper side of the desired gain point.</param>
        /// <param name="factor">Variable determinig how and when the interpolation is done.</param>
        /// <returns>
        /// Vector containing the lift, drag and moment coefficients
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector3 EvaluateCoefficients(SilantroAirfoil rootfoil, SilantroAirfoil tipfoil, float α, float panelSpan, float foilLength)
        {
            Vector3 coefficients = Vector3.zero;

            // ----------------------------- Lift
            float rootLiftCoefficient = rootfoil.liftCurve.Evaluate(α);
            float tipLiftCoefficient = tipfoil.liftCurve.Evaluate(α);
            coefficients.x = MathBase.EstimateEffectiveValue(rootLiftCoefficient, tipLiftCoefficient, panelSpan, 0, foilLength);


            // ----------------------------- Drag
            float rootDragCoefficient = rootfoil.dragCurve.Evaluate(α);
            float tipDragoefficient = tipfoil.dragCurve.Evaluate(α);
            coefficients.y = MathBase.EstimateEffectiveValue(rootDragCoefficient, tipDragoefficient, panelSpan, 0, foilLength);


            // ----------------------------- Moment
            float rootMomentCoefficient = rootfoil.momentCurve.Evaluate(α);
            float tipMomentoefficient = tipfoil.momentCurve.Evaluate(α);
            coefficients.z = MathBase.EstimateEffectiveValue(rootMomentCoefficient, tipMomentoefficient, panelSpan, 0, foilLength);

            return coefficients;
        }
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector2 XZ(this Vector3 v_3D) { Vector2 v_2D = new Vector2(v_3D.x, v_3D.z); return v_2D;}
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector3 XYZ(this Vector2 v_2D) { Vector3 v_3D = new Vector3(v_2D.x, 0f, v_2D.y); return v_3D; }
    }
}

















// ----------------------------------------------------------------------------------------------------------------------------------------------------------
namespace Oyedoyin.Navigation
{

    [System.Serializable]
    public class Aircraft
    {
        public Vector3 position;
        public float heading;
        public float turnRadius;
        public float speed;
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public static class PathGenerator
    {
        public static Vector3 baseLeftReference;
        public static Vector3 baseRightReference;
        public static Vector3 targetLeftReference;
        public static Vector3 targetRightReference;

        static Vector3 basePosition, finalPosition;
        static float baseHeading, finalHeading;
        static List<SilantroPath> pathDataList = new List<SilantroPath>();







        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static SilantroPath GetPath(Vector3 currentPosition, float currentHeading, Vector3 targetPosition, float targetHeading, float turnRadius, float stepDistance)
        {
            basePosition = currentPosition; finalPosition = targetPosition;
            baseHeading = currentHeading; finalHeading = targetHeading;
            pathDataList.Clear();
            SilantroPath path;

            // --------------------------------- Calls
            PositionDubinCircles(turnRadius);
            CalculateDubinsPathsLengths(turnRadius);

            if (pathDataList.Count > 0)
            {
                path = pathDataList[0];
                foreach (SilantroPath pathX in pathDataList) { if (pathX.pathLength < path.pathLength) { path = pathX; } }

                //Generate the final coordinates of the path from tangent points and segment lengths
                GetPathVectors(path, turnRadius, stepDistance);
                return path;
            }

            return null;
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static List<Vector3> GenerateWaypointCorner(Transform pointA, Transform pointB, Transform pointC, float turnRadius, float segmentSpacing)
        {
            List<Vector3> pathPoints = new List<Vector3> { pointA.position };
            Vector2 A = new Vector2(pointA.position.x, pointA.position.z);
            Vector2 B = new Vector2(pointB.position.x, pointB.position.z);
            Vector2 C = new Vector2(pointC.position.x, pointC.position.z);
            float headingChange = Vector2.Angle(A - B, C - B);

            float junctionDistance = turnRadius * Mathf.Tan(((180 - headingChange) * Mathf.Deg2Rad) / 2);
            float lengthAB = Vector2.Distance(A, B);
            float lengthBC = Vector2.Distance(B, C);
            float alpha1 = junctionDistance / lengthAB; alpha1 = Mathf.Clamp(alpha1, 0, 1);
            float alpha2 = junctionDistance / lengthBC; alpha2 = Mathf.Clamp(alpha2, 0, 1);
            Vector3 linePointAB = MathBase.EstimateSectionPosition(pointA.position, pointB.position, alpha1);
            pathPoints.Add(linePointAB);
            Vector3 linePointBC = MathBase.EstimateSectionPosition(pointB.position, pointC.position, alpha2);

            float baseHeading = pointA.eulerAngles.y * Mathf.Deg2Rad;
            float finalHeading = pointC.eulerAngles.y * Mathf.Deg2Rad;

            SilantroPath dataPath = PathGenerator.GetPath(linePointAB, baseHeading, linePointBC, finalHeading, turnRadius, segmentSpacing);
            if (dataPath != null && dataPath.pathVectorPoints.Count > 1)
            { foreach (Vector3 point in dataPath.pathVectorPoints) { pathPoints.Add(point); } }
            pathPoints.Add(linePointBC);
            pathPoints.Add(pointC.position);

            return pathPoints;
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static List<Vector3> GenerateWaypointConnection(Transform pointA, Transform pointB, Transform pointC, float turnRadius, float segmentSpacing)
        {
            List<Vector3> pathPoints = new List<Vector3>();

            pathPoints.Add(pointA.position);
            Vector2 A = new Vector2(pointA.position.x, pointA.position.z);
            Vector2 B = new Vector2(pointB.position.x, pointB.position.z);
            Vector2 C = new Vector2(pointC.position.x, pointC.position.z);
            float headingChange = Vector2.Angle(A - B, C - B);

            float junctionDistance = turnRadius * Mathf.Tan(((180 - headingChange) * Mathf.Deg2Rad) / 2);
            float lengthAB = Vector2.Distance(A, B);
            float lengthBC = Vector2.Distance(B, C);
            float alpha1 = junctionDistance / lengthAB; alpha1 = Mathf.Clamp(alpha1, 0, 1);
            float alpha2 = junctionDistance / lengthBC; alpha2 = Mathf.Clamp(alpha2, 0, 1);
            Vector3 linePointAB = MathBase.EstimateSectionPosition(pointA.position, pointB.position, alpha1);
            Vector2 AB = new Vector2(linePointAB.x, linePointAB.z);
            float length1 = Vector2.Distance(A, AB);

            // --------------------------------------- Get Points between A and LAB

            int segments1 = Mathf.FloorToInt(length1 / segmentSpacing);
            List<Vector3> LAB = MathBase.DivideConnection(pointA.position, linePointAB, segments1);
            if (LAB != null && LAB.Count > 1)
            { foreach (Vector3 point in LAB) { pathPoints.Add(point); } }


            Vector3 linePointBC = MathBase.EstimateSectionPosition(pointB.position, pointC.position, alpha2);
            Vector2 BC = new Vector2(linePointBC.x, linePointBC.z);
            float length2 = Vector2.Distance(BC, C);

            float baseHeading = pointA.eulerAngles.y * Mathf.Deg2Rad;
            float finalHeading = pointC.eulerAngles.y * Mathf.Deg2Rad;

            SilantroPath dataPath = GetPath(linePointAB, baseHeading, linePointBC, finalHeading, turnRadius, segmentSpacing);
            if (dataPath != null && dataPath.pathVectorPoints.Count > 1)
            {foreach (Vector3 point in dataPath.pathVectorPoints) { pathPoints.Add(point); } }

            int segments2 = Mathf.FloorToInt(length2 / segmentSpacing);
            List<Vector3> LBC = MathBase.DivideConnection(linePointBC, pointC.position, segments2);
            if (LBC != null && LBC.Count > 1)
            { foreach (Vector3 point in LBC) { pathPoints.Add(point); } }


            // ------------------------------------ Filter and Return
            pathPoints = pathPoints.Distinct().ToList();
            return pathPoints;
        }





        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        static void PositionDubinCircles(float turnRadius)
        {
            targetRightReference = MathBase.GetRightCircleCenterPos(finalPosition, finalHeading, turnRadius);
            targetLeftReference = MathBase.GetLeftCircleCenterPos(finalPosition, finalHeading, turnRadius);
            baseRightReference = MathBase.GetRightCircleCenterPos(basePosition, baseHeading, turnRadius);
            baseLeftReference = MathBase.GetLeftCircleCenterPos(basePosition, baseHeading, turnRadius);
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        static void CalculateDubinsPathsLengths(float turnRadius)
        {
            if (baseRightReference.x != targetRightReference.x && baseRightReference.z != targetRightReference.z){ Get_RSR_Length(turnRadius); }
            if (baseLeftReference.x != targetLeftReference.x && baseLeftReference.z != targetLeftReference.z) { Get_LSL_Length(turnRadius); }
            float comparisonSqr = turnRadius * 2f * turnRadius * 2f;

            if ((baseRightReference - targetLeftReference).sqrMagnitude > comparisonSqr) { Get_RSL_Length(turnRadius); }
            if ((baseLeftReference - targetRightReference).sqrMagnitude > comparisonSqr) { Get_LSR_Length(turnRadius); }
            comparisonSqr = 4f * turnRadius * 4f * turnRadius;

            if ((baseRightReference - targetRightReference).sqrMagnitude < comparisonSqr) { Get_RLR_Length(turnRadius); }
            if ((baseLeftReference - targetLeftReference).sqrMagnitude < comparisonSqr) { Get_LRL_Length(turnRadius); }
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        static void Get_RSR_Length(float turnRadius)
        {
            Vector3 baseTangent, finalTangent;
            MathBase.LSLorRSR(baseRightReference, targetRightReference, false, out baseTangent, out finalTangent, turnRadius);

            //Calculate lengths
            float length1 = MathBase.GetArcLength(baseRightReference, basePosition, baseTangent, false, turnRadius);
            float length2 = (baseTangent - finalTangent).magnitude;
            float length3 = MathBase.GetArcLength(targetRightReference, finalTangent, finalPosition, false, turnRadius);

            //Save the data
            SilantroPath pathData = new SilantroPath(length1, length2, length3, baseTangent, finalTangent);
            pathData.segment2Turning = false;
            pathData.SetIfTurningRight(true, false, true);
            pathDataList.Add(pathData);
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        static void Get_LSL_Length(float turnRadius)
        {
            Vector3 baseTangent, finalTangent;
            MathBase.LSLorRSR(baseLeftReference, targetLeftReference, true, out baseTangent, out finalTangent, turnRadius);

            //Calculate lengths
            float length1 = MathBase.GetArcLength(baseLeftReference, basePosition, baseTangent, true, turnRadius);
            float length2 = (baseTangent - finalTangent).magnitude;
            float length3 = MathBase.GetArcLength(targetLeftReference, finalTangent, finalPosition, true, turnRadius);

            //Save the data
            SilantroPath pathData = new SilantroPath(length1, length2, length3, baseTangent, finalTangent);
            pathData.segment2Turning = false;
            pathData.SetIfTurningRight(false, false, false);
            pathDataList.Add(pathData);
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        static void Get_RSL_Length(float turnRadius)
        {
            Vector3 baseTangent, finalTangent;
            MathBase.RSLorLSR(baseRightReference, targetLeftReference, false, out baseTangent, out finalTangent, turnRadius);

            //Calculate lengths
            float length1 = MathBase.GetArcLength(baseRightReference, basePosition, baseTangent, false, turnRadius);
            float length2 = (baseTangent - finalTangent).magnitude;
            float length3 = MathBase.GetArcLength(targetLeftReference, finalTangent, finalPosition, true, turnRadius);

            //Save the data
            SilantroPath pathData = new SilantroPath(length1, length2, length3, baseTangent, finalTangent);
            pathData.segment2Turning = false;
            pathData.SetIfTurningRight(true, false, false);
            pathDataList.Add(pathData);
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        static void Get_LSR_Length(float turnRadius)
        {
            Vector3 baseTangent, finalTangent;
            MathBase.RSLorLSR(baseLeftReference, targetRightReference, true, out baseTangent, out finalTangent, turnRadius);

            //Calculate lengths
            float length1 = MathBase.GetArcLength(baseLeftReference, basePosition, baseTangent, true, turnRadius);
            float length2 = (baseTangent - finalTangent).magnitude;
            float length3 = MathBase.GetArcLength(targetRightReference, finalTangent, finalPosition, false, turnRadius);

            //Save the data
            SilantroPath pathData = new SilantroPath(length1, length2, length3, baseTangent, finalTangent);
            pathData.segment2Turning = false;
            pathData.SetIfTurningRight(false, false, true);
            pathDataList.Add(pathData);
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        static void Get_RLR_Length(float turnRadius)
        {
            Vector3 baseTangent, finalTangent, middleCircle;
            MathBase.GetRLRorLRLTangents(baseRightReference, targetRightReference, false, out baseTangent, out finalTangent, out middleCircle, turnRadius);

            //Calculate lengths
            float length1 = MathBase.GetArcLength(baseRightReference, basePosition, baseTangent, false, turnRadius);
            float length2 = MathBase.GetArcLength(middleCircle, baseTangent, finalTangent, true, turnRadius);
            float length3 = MathBase.GetArcLength(targetRightReference, finalTangent, finalPosition, false, turnRadius);

            //Save the data
            SilantroPath pathData = new SilantroPath(length1, length2, length3, baseTangent, finalTangent);
            pathData.segment2Turning = true;
            pathData.SetIfTurningRight(true, false, true);
            pathDataList.Add(pathData);
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        static void Get_LRL_Length(float turnRadius)
        {
            Vector3 baseTangent, finalTangent, middleCircle;
            MathBase.GetRLRorLRLTangents(baseLeftReference, targetLeftReference, true, out baseTangent, out finalTangent, out middleCircle, turnRadius);

            //Calculate the total length of this path
            float length1 = MathBase.GetArcLength(baseLeftReference, basePosition, baseTangent, true, turnRadius);
            float length2 = MathBase.GetArcLength(middleCircle, baseTangent, finalTangent, false, turnRadius);
            float length3 = MathBase.GetArcLength(targetLeftReference, finalTangent, finalPosition, true, turnRadius);

            //Save the data
            SilantroPath pathData = new SilantroPath(length1, length2, length3, baseTangent, finalTangent);
            pathData.segment2Turning = true;
            pathData.SetIfTurningRight(false, true, false);
            pathDataList.Add(pathData);
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        static void GetPathVectors(SilantroPath pathData, float turnRadius, float stepDistance)
        {
            //Store the waypoints of the final path here
            List<Vector3> finalPath = new List<Vector3>();


            Vector3 currentPos = basePosition;
            float theta = baseHeading;
            finalPath.Add(currentPos);
            int segments = 0;

            //First
            segments = Mathf.FloorToInt(pathData.lengthA / stepDistance);
            MathBase.AddCoordinatesToPath(ref currentPos, ref theta, finalPath, segments, true, pathData.segment1TurningRight, turnRadius, stepDistance);

            //Second
            segments = Mathf.FloorToInt(pathData.lengthB / stepDistance);
            MathBase.AddCoordinatesToPath(ref currentPos, ref theta, finalPath, segments, pathData.segment2Turning, pathData.segment2TurningRight, turnRadius, stepDistance);

            //Third
            segments = Mathf.FloorToInt(pathData.lengthC / stepDistance);
            MathBase.AddCoordinatesToPath(ref currentPos, ref theta, finalPath, segments, true, pathData.segment3TurningRight, turnRadius, stepDistance);
            finalPath.Add(new Vector3(finalPosition.x, currentPos.y, finalPosition.z));
           
            pathData.pathVectorPoints = finalPath;
        }
    }
}

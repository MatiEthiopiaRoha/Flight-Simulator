using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Oyedoyin;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif



namespace Silantro
{
    public class FMath
    {

        public static AnimationCurve PlotControlEffectiveness()
        {
            AnimationCurve ƞ = new AnimationCurve() ;
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
        public static void PlotAirfoil(Vector3 leadingPoint, Vector3 trailingPoint, SilantroAirfoil foil, Transform foilTransform, out float foilArea, out List<Vector3> points)
        {
            points = new List<Vector3>(); List<float> xt = new List<float>(); float chordDistance = Vector3.Distance(leadingPoint, trailingPoint);
            Vector3 PointA = Vector3.zero, PointXA = Vector3.zero, PointXB = Vector3.zero, PointB = Vector3.zero;

            //FIND POINTS
            if (foil.x.Count > 0)
            {
                for (int j = 0; (j < foil.x.Count); j++)
                {
                    //BASE POINT
                    Vector3 XA = leadingPoint - ((leadingPoint - trailingPoint).normalized * (foil.x[j] * chordDistance)); Vector3 liftDirection = foilTransform.up.normalized;
                    PointA = XA + (liftDirection * ((foil.y[j]) * chordDistance)); points.Add(PointA); if ((j + 1) < foil.x.Count) { Vector3 XB = (leadingPoint - ((leadingPoint - trailingPoint).normalized * (foil.x[j + 1] * chordDistance))); PointB = XB + (liftDirection.normalized * ((foil.y[j + 1]) * chordDistance)); }
                    //CONNECT
                    Gizmos.color = Color.white; Gizmos.DrawLine(PointA, PointB);
                }
            }

            //PERFORM CALCULATIONS
            xt = new List<float>();
            for (int j = 0; (j < points.Count); j++) { xt.Add(Vector3.Distance(points[j], points[(points.Count - j - 1)])); Gizmos.DrawLine(points[j], points[(points.Count - j - 1)]); }
            foilArea = Mathf.Pow(chordDistance, 2f) * (((foil.xtc * 0.01f) + 3) / 6f) * (foil.tc * 0.01f);
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void PlotRibAirfoil(Vector3 leadingPoint, Vector3 trailingPoint, float distance, float wingTip, Color ribColor, bool drawSplits, SilantroAirfoil rootAirfoil, SilantroAirfoil tipAirfoil, float span, Transform foilTransform)
        {
            List<Vector3> points = new List<Vector3>(); List<float> xt = new List<float>(); float chordDistance = Vector3.Distance(leadingPoint, trailingPoint);
            Vector3 PointA = Vector3.zero, PointXA = Vector3.zero, PointXB = Vector3.zero;
            //FIND POINTS
            if (rootAirfoil.x.Count > 0)
            {
                for (int j = 0; (j < rootAirfoil.x.Count); j++)
                {
                    float xi = MathBase.EstimateEffectiveValue(rootAirfoil.x[j], tipAirfoil.x[j], distance, wingTip, span); float yi = MathBase.EstimateEffectiveValue(rootAirfoil.y[j], tipAirfoil.y[j], distance, wingTip, span);
                    //BASE POINT
                    Vector3 XA = leadingPoint - ((leadingPoint - trailingPoint).normalized * (xi * chordDistance)); Vector3 liftDirection = foilTransform.up; PointXA = XA + (liftDirection * yi * chordDistance); points.Add(PointXA);
                    if ((j + 1) < rootAirfoil.x.Count)
                    {
                        float xii = MathBase.EstimateEffectiveValue(rootAirfoil.x[j + 1], tipAirfoil.x[j + 1], distance, wingTip, span); float yii = MathBase.EstimateEffectiveValue(rootAirfoil.y[j + 1], tipAirfoil.y[j + 1], distance, wingTip, span);
                        Vector3 XB = (leadingPoint - ((leadingPoint - trailingPoint).normalized * (xii * chordDistance))); PointXB = XB + (liftDirection.normalized * (yii * chordDistance));
                    }
                    //CONNECT
                    Gizmos.color = ribColor; Gizmos.DrawLine(PointXA, PointXB);
                }
            }
            if (drawSplits) { xt = new List<float>(); for (int jx = 0; (jx < points.Count); jx++) { xt.Add(Vector3.Distance(points[jx], points[(points.Count - jx - 1)])); Gizmos.color = ribColor; Gizmos.DrawLine(points[jx], points[(points.Count - jx - 1)]); } }
        }






        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void EstimateControlExtension(Vector3 LeadEdgeLeft, Vector3 TrailEdgeRight, Vector3 TrailEdgeLeft, Vector3 LeadEdgeRight, float inputRootChord, float inputTipChord, float deflection, out Vector3 leftControlExtension, out Vector3 rightControlExtension)
        {
            leftControlExtension = (Quaternion.AngleAxis(deflection, ((MathBase.EstimateSectionPosition(TrailEdgeRight, LeadEdgeRight, (inputTipChord * 0.01f)) -
            MathBase.EstimateSectionPosition(TrailEdgeLeft, LeadEdgeLeft, (inputRootChord * 0.01f))).normalized))) * (TrailEdgeLeft - MathBase.EstimateSectionPosition(TrailEdgeLeft, LeadEdgeLeft, (inputRootChord * 0.01f)));
            rightControlExtension = (Quaternion.AngleAxis(deflection, ((MathBase.EstimateSectionPosition(TrailEdgeRight, LeadEdgeRight, (inputTipChord * 0.01f)) -
            MathBase.EstimateSectionPosition(TrailEdgeLeft, LeadEdgeLeft, (inputRootChord * 0.01f))).normalized))) * (TrailEdgeRight - MathBase.EstimateSectionPosition(TrailEdgeRight, LeadEdgeRight, (inputTipChord * 0.01f)));
        }

    }
}

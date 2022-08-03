
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroBody : MonoBehaviour
{
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[HideInInspector] public float maximumDiameter = 20f;[HideInInspector] public int resolution = 10; GameObject point;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[Serializable]
	public class SectionPoint
	{
		public Transform sectionTransform;
		public float sectionDiameterPercentage = 10;
		public float sectionHeightPercentage = 10;
		[HideInInspector] public List<Vector3> sectionPointList;
		public float height, width;
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[HideInInspector] public List<SectionPoint> sectionPoints;
	[HideInInspector] public float totalArea, aircraftLength;
	[HideInInspector] public float skinDragCoefficient;
	public enum SurfaceFinish { SmoothPaint, PolishedMetal, ProductionSheetMetal, MoldedComposite, PaintedAluminium }
	[HideInInspector] public SurfaceFinish surfaceFinish = SurfaceFinish.PaintedAluminium;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[HideInInspector] public SilantroController controller;
	[HideInInspector] public Rigidbody aircraft;
	[HideInInspector] public float airspeed = 10f, k, knotSpeed, totalDrag, RE;


	[HideInInspector] public Transform horizontalReference, verticalReference;
	[HideInInspector] public Vector3 cogPosition;

	bool allOk;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeBody()
	{

		//----------------------------
		_checkPrerequisites();


		if (allOk)
		{
			//SET FINISH FACTOR
			if (surfaceFinish == SurfaceFinish.MoldedComposite) { k = 0.17f; }
			if (surfaceFinish == SurfaceFinish.PaintedAluminium) { k = 3.33f; }
			if (surfaceFinish == SurfaceFinish.PolishedMetal) { k = 0.50f; }
			if (surfaceFinish == SurfaceFinish.ProductionSheetMetal) { k = 1.33f; }
			if (surfaceFinish == SurfaceFinish.SmoothPaint) { k = 2.08f; }

			//CREATE LIFT REFERENCES
			GameObject horizontalRef = new GameObject("Horizontal Reference"); horizontalReference = horizontalRef.transform; horizontalReference.parent = this.transform; horizontalReference.localPosition = Vector3.zero;
			GameObject verticaRef = new GameObject("Vertical Reference"); verticalReference = verticaRef.transform; verticalReference.parent = this.transform; verticalReference.localPosition = Vector3.zero;
			Quaternion veritcalRotation = Quaternion.identity; veritcalRotation.eulerAngles = new Vector3(0, 0, 90f);
			verticalReference.localRotation = veritcalRotation;
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	protected void _checkPrerequisites()
	{
		//CHECK COMPONENTS
		if (controller != null && aircraft != null)
		{
			allOk = true;
		}
		else if (aircraft == null)
		{
			Debug.LogError("Prerequisites not met on " + transform.name + "....Aircraft rigidbody not assigned");
			allOk = false;
		}
		else if (controller == null)
		{
			Debug.LogError("Prerequisites not met on " + transform.name + "....controller not assigned");
			allOk = false;
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		if (controller != null)
		{
			airspeed = aircraft.velocity.magnitude; knotSpeed = airspeed * 1.944f;
			Vector3 dragForce = -aircraft.velocity; dragForce.Normalize();
			cogPosition = aircraft.transform.TransformPoint(aircraft.centerOfMass);

			if (airspeed > 0)
			{
				skinDragCoefficient = EstimateSkinDragCoefficient(airspeed);
				totalDrag = 0.5f * controller.core.airDensity * airspeed * airspeed * totalArea * skinDragCoefficient;
			}


			//CALCULATE BODY DRAG
			dragForce *= totalDrag; if (totalDrag > 0) { aircraft.AddForce(dragForce, ForceMode.Force); }
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float EstimateRe(float inputSpeed)
	{
		float Re1 = (controller.core.airDensity * inputSpeed * aircraftLength) / controller.core.viscocity; float Re2;
		if (controller.core.machSpeed < 0.9f) { Re2 = 38.21f * Mathf.Pow(((aircraftLength * 3.28f) / (k / 100000)), 1.053f); }
		else { Re2 = 44.62f * Mathf.Pow(((aircraftLength * 3.28f) / (k / 100000)), 1.053f) * Mathf.Pow(controller.core.machSpeed, 1.16f); }
		float superRe = Mathf.Min(Re1, Re2); RE = superRe; return superRe;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float EstimateSkinDragCoefficient(float velocity)
	{
		float Recr = EstimateRe(velocity);
		float baseCf = frictionDragCurve.Evaluate(Recr) / 1000f;

		//WRAPPING CORRECTION
		float Cfa = baseCf * (0.0025f * (aircraftLength / maximumDiameter) * Mathf.Pow(Recr, -0.2f));
		//SUPERVELOCITY CORRECTION
		float Cfb = baseCf * Mathf.Pow((maximumDiameter / aircraftLength), 1.5f);
		//PRESSURE CORRECTION
		float Cfc = baseCf * 7 * Mathf.Pow((maximumDiameter / aircraftLength), 3f);
		float actualCf = 1.03f * (baseCf + Cfa + Cfb + Cfc);
		return actualCf;
	}


	[HideInInspector] public AnimationCurve frictionDragCurve;
	[HideInInspector] public List<float> sectionAreas = new List<float>();
	[HideInInspector] public float maximumCrossArea, maximumSectionDiameter, finenessRatio;[HideInInspector] public int layer;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{

		frictionDragCurve = new AnimationCurve();
		frictionDragCurve.AddKey(new Keyframe(1000000000, 1.5f));
		frictionDragCurve.AddKey(new Keyframe(100000000, 2.0f));
		frictionDragCurve.AddKey(new Keyframe(10000000, 2.85f));
		frictionDragCurve.AddKey(new Keyframe(1000000, 4.1f));
		frictionDragCurve.AddKey(new Keyframe(100000, 7.0f));


		if (sectionPoints != null && sectionPoints.Count > 0)
		{
			for (int a = 0; a < sectionPoints.Count; a++)
			{
				if (sectionPoints[a].sectionTransform == null)
				{
					sectionPoints.Remove(sectionPoints[a]);
				}
			}


			sectionAreas = new List<float>();
			for (int i = 0; i < sectionPoints.Count - 1; i++)
			{
				//AREA
				float sectionWidth = sectionPoints[i].width; float sectionHeight = sectionPoints[i].height;
				float area = 3.142f * sectionHeight * sectionWidth; sectionAreas.Add(area); maximumCrossArea = sectionAreas.Max();
				layer = sectionAreas.IndexOf(maximumCrossArea);
			}

			//EQUIVALENT DIAMETER
			float sectionWidthM = sectionPoints[layer].width;
			float sectionHeightM = sectionPoints[layer].height;
			float perimeter = 6.284f * Mathf.Pow((0.5f * (Mathf.Pow(sectionHeightM, 2) + Mathf.Pow(sectionWidthM, 2))), 0.5f);
			maximumSectionDiameter = (1.55f * Mathf.Pow(maximumCrossArea, 0.625f)) / Mathf.Pow(perimeter, 0.25f);
			float sectionArea;


			totalArea = 0f;
			foreach (SectionPoint position in sectionPoints)
			{

				if (position.sectionTransform != null)
				{
					position.height = (maximumDiameter * position.sectionHeightPercentage * 0.01f) / 2;
					position.width = (maximumDiameter * position.sectionDiameterPercentage * 0.01f) / 2;
					DrawEllipse(position.sectionTransform, position.width, position.height, out position.sectionPointList);
				}
			}

			if (sectionPoints.Count > 0)
			{
				for (int a = 0; a < sectionPoints.Count - 1; a++)
				{
					DrawConnection(sectionPoints[a].sectionPointList, sectionPoints[a + 1].sectionPointList, out sectionArea);
					totalArea += sectionArea;
				}
				aircraftLength = Vector3.Distance(sectionPoints[0].sectionTransform.position, sectionPoints[sectionPoints.Count - 1].sectionTransform.position);
				float noseArea = 3.142f * sectionPoints[0].height * sectionPoints[0].width;
				totalArea += noseArea;
			}
			finenessRatio = aircraftLength / maximumSectionDiameter;
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void DrawEllipse(Transform positionTransform, float a, float c, out List<Vector3> outList)
	{
		outList = new List<Vector3>();
		Quaternion q1 = Quaternion.AngleAxis(90, positionTransform.right);
		for (float i = 0; i < 2 * Mathf.PI; i += 2 * Mathf.PI / resolution)
		{
			var newPoint = positionTransform.position + (q1 * positionTransform.rotation * (new Vector3(a * Mathf.Cos(i), 0, c * Mathf.Sin(i))));
			var lastPoint = positionTransform.position + (q1 * positionTransform.rotation * (new Vector3(a * Mathf.Cos(i + 2 * Mathf.PI / resolution), 0, c * Mathf.Sin(i + 2 * Mathf.PI / resolution))));
			Handles.DrawLine(newPoint, lastPoint);
			Gizmos.color = Color.red; outList.Add(newPoint);
			Gizmos.DrawSphere(newPoint, 0.02f);
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void DrawConnection(List<Vector3> pointsA, List<Vector3> pointsB, out float area)
	{
		area = 0f; float sectionArea;
		if (pointsA.Count == pointsB.Count && pointsA.Count > 0)
		{
			for (int i = 0; i < pointsA.Count - 1; i++)
			{
				Handles.color = new Color(1, 0, 0, 0.3f);
				Handles.DrawLine(pointsA[i], pointsB[i]);
				Handles.DrawAAConvexPolygon(pointsA[i], pointsA[i + 1], pointsB[i + 1], pointsB[i]);
				sectionArea = EstimatePanelSectionArea(pointsA[i], pointsA[i + 1], pointsB[i + 1], pointsB[i]);
				area += sectionArea;
			}
			//DRAW FROM END BACK TO START
			Handles.DrawAAConvexPolygon(pointsA[pointsA.Count - 1], pointsA[0], pointsB[0], pointsB[pointsA.Count - 1]);
			float closeArea = EstimatePanelSectionArea(pointsA[pointsA.Count - 1], pointsA[0], pointsB[0], pointsB[pointsA.Count - 1]);
			area += closeArea;
		}
	}
#endif


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void AddElement()
	{
		if (sectionPoints != null) { point = new GameObject("Section " + (sectionPoints.Count + 1)); }
		if (sectionPoints == null) { sectionPoints = new List<SectionPoint>(); point = new GameObject("Section 1"); }
		point.transform.parent = this.transform; point.transform.localPosition = Vector3.zero;
		if (sectionPoints != null && sectionPoints.Count > 1)
		{
			Vector3 predisessorPosition = sectionPoints[sectionPoints.Count - 1].sectionTransform.localPosition;
			point.transform.localPosition = new Vector3(predisessorPosition.x, predisessorPosition.y, predisessorPosition.z - 0.5f);
		}
		SectionPoint dragPoint = new SectionPoint
		{
			sectionTransform = point.transform
		};
		sectionPoints.Add(dragPoint);
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void AddSupplimentElement(float zFloat)
	{
		if (sectionPoints != null) { point = new GameObject("Section " + (sectionPoints.Count + 1)); }
		point.transform.parent = this.transform; point.transform.localPosition = new Vector3(0, 0, zFloat);
		SectionPoint dragPoint = new SectionPoint { sectionTransform = point.transform }; sectionPoints.Add(dragPoint);
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private float EstimatePanelSectionArea(Vector3 panelLeadingLeft, Vector3 panelLeadingRight, Vector3 panelTrailingLeft, Vector3 panelTrailingRight)
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
	//CALCULATE FACTOR POSITION
	public Vector3 EstimateSectionPosition(Vector3 lhs, Vector3 rhs, float factor) { Vector3 estimatedPosition = lhs + ((rhs - lhs) * factor); return estimatedPosition; }
}



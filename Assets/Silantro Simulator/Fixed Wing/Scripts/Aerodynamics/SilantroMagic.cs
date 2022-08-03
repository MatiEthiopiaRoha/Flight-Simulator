using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class SilantroMagic : MonoBehaviour
{

#if UNITY_EDITOR
	[HideInInspector] public Transform Instance;
	private Transform localInstance;
	public enum OperationMode
	{
		LeftReference,
		RightDuplicate
	}
	[HideInInspector] public OperationMode operationMode = OperationMode.LeftReference;
	[HideInInspector] public SilantroAerofoil currentFoil;
	[HideInInspector] public SilantroAerofoil referenceFoil;
	SilantroAerofoil leftAerofoil;




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Start()
	{
		localInstance = this.gameObject.transform;
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		//
		if (Instance != null && localInstance.root.gameObject == Instance.root.gameObject)
		{
			if (this == Instance)
			{
				return;
			}
			//SET POSITION
			transform.localScale = new Vector3(-Instance.transform.localScale.x, Instance.transform.localScale.y, Instance.transform.localScale.z);
			transform.localPosition = Instance.transform.localPosition;
			transform.localPosition = new Vector3(-transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
			//SET ROTATION
			transform.localRotation = new Quaternion(-Instance.transform.localRotation.x, Instance.transform.localRotation.y, Instance.transform.localRotation.z, Instance.transform.localRotation.w * -1.0f);
			//------------------------------------------
			if (referenceFoil != null)
			{
				if (referenceFoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing)
				{
					//SET ALLIGNMET
					currentFoil.wingPosition = referenceFoil.wingPosition;
					//SET POSITION
					if (referenceFoil.wingPosition == SilantroAerofoil.WingPosition.Right) { currentFoil.wingPosition = SilantroAerofoil.WingPosition.Left; }
				}

			}
			else
			{
				if (Instance != null) { referenceFoil = Instance.gameObject.GetComponent<SilantroAerofoil>(); }
			}
		}
	}












	// -------------------------------------------------------OPERATIONS------------------------------------------------------------------------------------------
	//TRANSFORM SETTINGS
	public void RotateX()
	{
		currentFoil.transform.Rotate(new Vector3(180, 0, 0));
	}
	public void RotateY()
	{
		currentFoil.transform.Rotate(new Vector3(0, 180, 0));
	}
	//BLADE SETTINGS
	public void ReverseX()
	{
		Vector3 scale = currentFoil.transform.localScale;
		currentFoil.transform.localScale = new Vector3(scale.x *= -1, scale.y, scale.z);
	}
	public void ReverseY()
	{
		Vector3 scale = currentFoil.transform.localScale;
		currentFoil.transform.localScale = new Vector3(scale.x, scale.y *= -1, scale.z);
	}
	public void ReverseZ()
	{
		Vector3 scale = currentFoil.transform.localScale;
		currentFoil.transform.localScale = new Vector3(scale.x, scale.y, scale.z *= -1);
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void Done()
	{
		DestroyImmediate(this);
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void CreateLeftWing()
	{
		GameObject leftFoil = Instantiate(this.gameObject, this.transform.position, Quaternion.identity, this.transform.parent);
		if (currentFoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing)
		{
			leftFoil.transform.name = "Left Wing";
			leftAerofoil = leftFoil.GetComponent<SilantroAerofoil>();
			leftAerofoil.wingPosition = SilantroAerofoil.WingPosition.Left;
			leftAerofoil.foilTipLeadingExtension *= -1f;
			leftAerofoil.foilTipTrailingExtension *= -1f;
		}
		if (currentFoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer)
		{
			leftFoil.transform.name = "Left Stabilizer";
			leftAerofoil = leftFoil.GetComponent<SilantroAerofoil>();
			leftAerofoil.stabilizerPosition = SilantroAerofoil.StabilizerPosition.Left;
		}
		//SET POSITION
		leftFoil.GetComponent<SilantroMagic>().Done();
		leftFoil.transform.localScale = new Vector3(-currentFoil.transform.localScale.x, currentFoil.transform.localScale.y, currentFoil.transform.localScale.z);
		leftFoil.transform.localPosition = currentFoil.transform.localPosition;
		leftFoil.transform.localPosition = new Vector3(-leftFoil.transform.localPosition.x, leftFoil.transform.localPosition.y, leftFoil.transform.localPosition.z);


		//SET ROTATION
		leftFoil.transform.localRotation = new Quaternion(-currentFoil.transform.localRotation.x, currentFoil.transform.localRotation.y, currentFoil.transform.localRotation.z, currentFoil.transform.localRotation.w * -1.0f);
	}

#endif
}







#if UNITY_EDITOR
[CustomEditor(typeof(SilantroMagic))]
public class SilantroMagicEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1f, 0.4f, 0);
	//
	SilantroMagic foil;
	SerializedObject foilObject;
	void OnEnable()
	{
		foil = (SilantroMagic)target;
		foilObject = new SerializedObject(foil);
		if (foil.currentFoil == null)
		{
			foil.currentFoil = foil.gameObject.GetComponent<SilantroAerofoil>();
		}
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector();
		foilObject.Update();
		
		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Aerofoil Operation", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		foil.operationMode = (SilantroMagic.OperationMode)EditorGUILayout.EnumPopup("Mode", foil.operationMode);
		//
		if (foil.operationMode == SilantroMagic.OperationMode.LeftReference)
		{
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Reference Aerofoil", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			foil.Instance = EditorGUILayout.ObjectField(" ", foil.Instance, typeof(Transform), true) as Transform;
			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Transform Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			if (GUILayout.Button("Flip X"))
			{
				foil.RotateX();
			}
			GUILayout.Space(2f);
			if (GUILayout.Button("Flip Y"))
			{
				foil.RotateY();
			}
			//
			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Foil Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			if (GUILayout.Button("Flip X"))
			{
				foil.ReverseX();
			}
			GUILayout.Space(2f);
			if (GUILayout.Button("Flip Y"))
			{
				foil.ReverseY();
			}
			GUILayout.Space(2f);
			if (GUILayout.Button("Flip Z"))
			{
				foil.ReverseZ();
			}
			//
			GUILayout.Space(15f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Remove Magic", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			if (GUILayout.Button("Done"))
			{
				foil.Done();
			}
		}
		if (foil.operationMode == SilantroMagic.OperationMode.RightDuplicate)
		{

			GUILayout.Space(15f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox(" ", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			if (GUILayout.Button("Create Left Foil"))
			{
				foil.CreateLeftWing();
				foil.Done();
			}
		}
	}
}
#endif
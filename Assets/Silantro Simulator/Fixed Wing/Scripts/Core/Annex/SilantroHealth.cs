using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroHealth : MonoBehaviour
{


	public enum ComponentType
	{
		Unspecified, Wing, Aircraft, FuelTank, Munition, Engine, Blade, GroundVehicle
	}
	[HideInInspector] public ComponentType componentType = ComponentType.Unspecified;
	//HEALTH VALUES
	[HideInInspector] public float maximumHealth = 100;
	[HideInInspector] public float currentHealth;
	[HideInInspector] public GameObject explosionPrefab;
	bool exploded;
	[HideInInspector] public SilantroHealth[] childenHealth;
	//ATTACHED COMPONENTS
	[System.Serializable]
	public class Model
	{
		public GameObject model; public float weight = 100;
	}
	[HideInInspector] public List<Model> attachments = new List<Model>();
	Vector3 dropVelocity;




	//--------------------------------------INIIALIZE HEALTH
	void Start()
	{
		currentHealth = maximumHealth;
	}





	//--------------------------------------//DAMAGE INPUT
	public void SilantroDamage(float input)
	{
		currentHealth -= Mathf.Abs(input);
		if (currentHealth < 0) { currentHealth = 0; }
		//DESTROY
		if (currentHealth == 0)
		{
			DestroyComponent();
		}
	}

	//--------------------------------------//COLLISION DAMAGE
	void OnCollisionEnter(Collision col)
	{

		if (!col.collider.transform.gameObject.GetComponent<SilantroMunition>())
		{
			//
			if (componentType == ComponentType.Aircraft)
			{
				Rigidbody aircraft = GetComponent<Rigidbody>();
				if (aircraft != null && aircraft.velocity.magnitude > 20f)
				{//EXPLODE IF SPEED > 20m/s
					DestroyComponent();
				}
			}
			else
			{
				Rigidbody component = GetComponent<Rigidbody>();
				if (component != null && component.velocity.magnitude > 5f)
				{//EXPLODE IF SPEED > 20m/s
					DestroyComponent();
				}
			}
		}
	}










	//--------------------------------------//DESTORY COMPONENT
	public void DestroyComponent()
	{
		//1. MUNITION
		if (componentType == ComponentType.Munition)
		{
			SilantroMunition munition = GetComponent<SilantroMunition>();
			if (munition != null)
			{
				//munition.Explode ("Health Call",munition.transform.position);
			}
		}
		//2. WING
		if (componentType == ComponentType.Wing)
		{
			SilantroAerofoil foil = GetComponent<SilantroAerofoil>();
			if (foil != null)
			{
				Destroy(foil);
			}
		}
		//3. MAIN AIRCRAFT
		if (componentType == ComponentType.Aircraft)
		{
			//DESTROY COMPONENTs WITH HEALTH
			childenHealth = GetComponentsInChildren<SilantroHealth>();
			foreach (SilantroHealth com in childenHealth)
			{
				if (com != null && com.componentType != ComponentType.Aircraft)
				{
					com.DestroyComponent();
				}
			}
			//GET CONTROLLER OBJECT
			SilantroController controller = GetComponent<SilantroController>();
			if (controller != null)
			{
				//DISABLE AIRCRAFT
				//0.1 REMOVE COCKPIT COMPONENTS
				SilantroDial[] dials = controller.gameObject.GetComponentsInChildren<SilantroDial>();
				foreach (SilantroDial dial in dials)
				{
					Destroy(dial.gameObject);
				}
				SilantroLever[] levers = controller.gameObject.GetComponentsInChildren<SilantroLever>();
				foreach (SilantroLever lever in levers)
				{
					Destroy(lever.gameObject);
				}
				//0. REMOVE WHEELS
				WheelCollider[] wheels = controller.GetComponentsInChildren<WheelCollider>();
				foreach (WheelCollider wheel in wheels)
				{
					Destroy(wheel.gameObject);
				}
				//1. REMOVE GEAR SYSTEM
				if (controller.gearHelper) { Destroy(controller.gearHelper); }
				//2. REMOVE HYDRAULIC SYSTEMS
				if (controller.hydraulics != null)
				{
					foreach (SilantroHydraulicSystem hydraulic in controller.hydraulics) { Destroy(hydraulic.gameObject); }
				}
				//3. REMOVE WINGS TO SAVE PERFORMANCE
				if (controller.wings != null)
				{
					foreach (SilantroAerofoil wing in controller.wings) { Destroy(wing); }
				}
				//4. ENABLE EXTERIOR CAMERA
				if (controller.view)
				{
					if (controller.view.normalInterior)
					{
						controller.view.ActivateExteriorCamera();
						controller.view.normalInterior.gameObject.SetActive(false);
					}
					//controller.view.isControllable = false;
				}

				//5 REMOVE CORE
				if (controller.core)
				{
					Destroy(controller.core.gameObject);
				}

				//6 REMOVE LIGHT SYSTEM
				foreach (SilantroLight light in controller.lightBulbs)
				{
					Destroy(light.gameObject);
				}

				//7. DETACH BLACK BOX
				//if (controller.blackBox) {
				//	controller.blackBox.StoreCSVData ();
				//	Destroy (controller.blackBox.gameObject);
				//}

				//8. REMOVE RADAR
				if (controller.radarCore)
				{
					Destroy(controller.radarCore.gameObject);
				}

				//9. REMOVE CONTROLLER SCRIPT
				Destroy(controller);
			}
		}
		//4. ENGINES
		if (componentType == ComponentType.Engine)
		{
			gameObject.SendMessageUpwards("ShutDownEngine", null, SendMessageOptions.DontRequireReceiver);
		}
		// DETACH MODELS
		//ADD COLLIDERS TO ATTACHED PARTS
		if (transform.root.GetComponent<Rigidbody>())
		{
			dropVelocity = transform.root.GetComponent<Rigidbody>().velocity;
		}
		//
		foreach (Model model in attachments)
		{
			//ADD COLLIDER
			if (model.model != null && model.model.GetComponent<BoxCollider>() == null)
			{
				model.model.AddComponent<BoxCollider>();
			}
			//ADD RIGIDBODY
			if (!model.model.GetComponent<Rigidbody>())
			{
				model.model.AddComponent<Rigidbody>();
			}
			//SET VALUES
			model.model.GetComponent<Rigidbody>().mass = model.weight;
			model.model.GetComponent<Rigidbody>().velocity = dropVelocity;
		}
		//MAKE COMPONENTS PHYSICS ENABLED
		if (GetComponent<Collider>() != null)
		{
			if (!gameObject.GetComponent<Rigidbody>())
			{
				Destroy(GetComponent<Collider>());
			}
			else
			{
				gameObject.GetComponent<Rigidbody>().isKinematic = false;
				gameObject.GetComponent<Rigidbody>().mass = 200f;
				gameObject.GetComponent<Rigidbody>().velocity = dropVelocity;
			}
		}
		//2. TRUCK
		if (componentType == ComponentType.GroundVehicle)
		{
			if (gameObject.GetComponent<Rigidbody>())
			{
				Destroy(gameObject.GetComponent<Rigidbody>());
				Destroy(GetComponent<Collider>());
			}
		}
		//1.5 UNSPECIFIED
		if (componentType != ComponentType.Munition)
		{
			if (explosionPrefab != null && !exploded)
			{
				GameObject explosion = Instantiate(explosionPrefab, this.transform.position, Quaternion.identity);
				explosion.transform.parent = this.transform; explosion.transform.localPosition = new Vector3(0, 0, 0);
				explosion.SetActive(true);
				explosion.GetComponentInChildren<AudioSource>().Play();
				exploded = true;
			}
		}
		///4. TRANSPONDER
		SilantroTransponder ponder = GetComponent<SilantroTransponder>();
		if (ponder != null)
		{
			Destroy(ponder);
		}
		//
		Destroy(GetComponent<SilantroHealth>());
	}
}










#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroHealth))]
public class SilantroHealthEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);
	SilantroHealth health;
	int listSize;
	SerializedProperty healthList;
	private static GUIContent deleteButton = new GUIContent("Remove", "Delete");
	private static GUILayoutOption buttonWidth = GUILayout.Width(60f);

	void OnEnable()
	{
		health = (SilantroHealth)target;
		healthList = serializedObject.FindProperty("attachments");
	}

	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		serializedObject.UpdateIfRequiredOrScript();

		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Health Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("componentType"), new GUIContent("Component"));
		GUILayout.Space(5f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumHealth"), new GUIContent("Maximum Health"));
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Current Health", health.currentHealth.ToString("0.0"));
		if (health.componentType != SilantroHealth.ComponentType.Munition)
		{
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionPrefab"), new GUIContent("Explosion Prefab"));
		}

		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Attachments Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		if (healthList != null)
		{
			EditorGUILayout.LabelField("Model Count", healthList.arraySize.ToString());
		}
		GUILayout.Space(3f);
		if (GUILayout.Button("Add Model"))
		{
			health.attachments.Add(new SilantroHealth.Model());
		}
		if (healthList != null)
		{
			GUILayout.Space(2f);
			//DISPLAY MODELS
			for (int i = 0; i < healthList.arraySize; i++)
			{
				SerializedProperty reference = healthList.GetArrayElementAtIndex(i);
				SerializedProperty model = reference.FindPropertyRelative("model");
				SerializedProperty weight = reference.FindPropertyRelative("weight");

				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Model : " + (i + 1).ToString(), MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(model);
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(weight);

				GUILayout.Space(3f);
				if (GUILayout.Button(deleteButton, EditorStyles.miniButtonRight, buttonWidth))
				{
					health.attachments.RemoveAt(i);
				}
				GUILayout.Space(5f);
			}
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
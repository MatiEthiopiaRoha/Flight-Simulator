//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public class SilantroExplosion : MonoBehaviour
{

	//PROPERTIES
	public float damage = 200f;
	public float explosionForce = 4000f;
	public float explosionRadius = 45f;
	float fractionalDistance;
	//LIGHT
	public AnimationCurve LightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	public float exposureTime = 1;
	public float lightIntensity = 5;
	private bool canUpdate;
	private float startTime;
	public Light lightSource;







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Start()
	{
		//EXPLOSION LIGHT
		lightSource = GetComponentInChildren<Light>();
		if (lightSource)
		{
			lightSource.intensity = LightCurve.Evaluate(0);
			startTime = Time.time;
			canUpdate = true;
		}
		//EFFECT
		Explode();
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void Explode()
	{
		//AQUIRE SURROUNDING COLLIDERS
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
		for (int i = 0; i < hitColliders.Length; i++)
		{
			Collider hit = hitColliders[i];
			if (hit != null)
			{
				//DISTANCE FALLOFF
				float distanceToObject = Vector3.Distance(transform.position, hit.gameObject.transform.position);
				fractionalDistance = (1 - (distanceToObject / explosionRadius));
				Vector3 exploionPosition = transform.position;
				//ONLY AFFECT OBJECTS WITHIN RANGE
				if (distanceToObject < explosionRadius)
				{
					//SEND DAMAGE MESSAGE
					float actualDamage = damage * fractionalDistance;
					hit.gameObject.SendMessageUpwards("SilantroDamage", (-actualDamage), SendMessageOptions.DontRequireReceiver);
					//FORCE
					//1. OBJECT ITSELF
					if (hit.GetComponent<Rigidbody>())
					{
						float actualForce = explosionForce * fractionalDistance;
						hit.GetComponent<Rigidbody>().AddExplosionForce(actualForce, transform.position, explosionRadius, 3f, ForceMode.Impulse);
					}
					//2. OBJECT PARENT
					else if (hit.transform.root.gameObject.GetComponent<Rigidbody>())
					{
						float actualForce = explosionForce * fractionalDistance;
						hit.transform.root.gameObject.GetComponent<Rigidbody>().AddExplosionForce(actualForce, transform.position, explosionRadius, (3.0f), ForceMode.Impulse);
					}
				}
			}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void Update()
	{
		if (lightSource)
		{
			float time = Time.time - startTime;
			if (canUpdate)
			{
				float eval = LightCurve.Evaluate(time / exposureTime) * lightIntensity;
				lightSource.intensity = eval;
			}
			if (time >= exposureTime)
			{
				canUpdate = false;
			}
		}
	}
}






#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroExplosion))]
public class SilantroExplosionEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);
	SilantroExplosion effect;

	private void OnEnable() { effect = (SilantroExplosion)target; }


	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		serializedObject.Update();



		GUILayout.Space(3f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Properties", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"), new GUIContent("Damage"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionForce"), new GUIContent("Explosion Force"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionRadius"), new GUIContent("Effective Radius"));


		GUILayout.Space(15f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Light Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("lightIntensity"), new GUIContent("Maximum Intensity"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("exposureTime"), new GUIContent("Exposure Time"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("LightCurve"), new GUIContent("Decay Curve"));

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
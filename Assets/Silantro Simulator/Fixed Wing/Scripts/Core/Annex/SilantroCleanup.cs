
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroCleanup : MonoBehaviour
{
	[HideInInspector] public float destroyTime = 5f;
	[HideInInspector] public bool contact;

	void Start() { Destroy(gameObject, destroyTime); }
	//DAMAGE
	void OnCollisionEnter(Collision col)
	{
		if (contact) { Destroy(gameObject); }
	}
}







#if UNITY_EDITOR
[CustomEditor(typeof(SilantroCleanup))]
public class SilantroDetroyerEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);

	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector();

		SilantroCleanup timer = (SilantroCleanup)target;

		GUILayout.Space(3f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Timer", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("destroyTime"), new GUIContent("Destroy Time"));
		GUILayout.Space(5f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("contact"), new GUIContent("Collision Destroy"));

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
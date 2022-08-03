//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class SilantroExtension : MonoBehaviour
{
	public enum Function
	{
		ImpactSound, CaseSound, SystemReset
	}
	[HideInInspector] public Function function = Function.ImpactSound;

	[HideInInspector] public AudioClip[] sounds;
	[HideInInspector] public float soundRange = 300f;
	[HideInInspector] private AudioSource audioOut;
	[Range(0, 1f)] public float soundVolume = 0.4f;
	[HideInInspector] public int soundCount = 1;

	//---------------------------------------------------------CASE IMPACT SOUND
	void OnCollisionEnter(Collision col)
	{
		if (function == Function.CaseSound)
		{
			if (col.collider.tag == "Ground")
			{
				if (audioOut && !audioOut.isPlaying)
				{
					audioOut.PlayOneShot(sounds[Random.Range(0, sounds.Length)]);
				}
			}
		}
	}

	//----------------------------------------------------------IMPACT EFFECT SOUND
	void Start()
	{
		if (function == Function.ImpactSound)
		{
			audioOut = gameObject.AddComponent<AudioSource>();
			audioOut.dopplerLevel = 0f;
			audioOut.spatialBlend = 1f;
			audioOut.rolloffMode = AudioRolloffMode.Custom;
			audioOut.maxDistance = soundRange;
			audioOut.volume = soundVolume;
			audioOut.PlayOneShot(sounds[Random.Range(0, sounds.Length)]);
		}
		if (function == Function.CaseSound)
		{
			audioOut = gameObject.AddComponent<AudioSource>();
			audioOut.dopplerLevel = 0f;
			audioOut.spatialBlend = 1f;
			audioOut.rolloffMode = AudioRolloffMode.Custom;
			audioOut.maxDistance = soundRange;
			audioOut.volume = soundVolume;
		}
	}




	//------------------------------------------------------------RESET SCENE
	public void ResetScene()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(this.gameObject.scene.name);
	}
}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroExtension))]
public class SilantroExtensionEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = Color.cyan;

	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector();
		serializedObject.Update();

		SilantroExtension extension = (SilantroExtension)target;

		GUILayout.Space(5f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("function"), new GUIContent("Function"));

		//1. SOUND SYSTEM
		if (extension.function == SilantroExtension.Function.CaseSound || extension.function == SilantroExtension.Function.ImpactSound)
		{
			GUILayout.Space(5f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Sounds", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(5f);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			GUIContent soundLabel = new GUIContent("Sound Clips");
			SerializedProperty muzs = this.serializedObject.FindProperty("sounds");
			EditorGUILayout.PropertyField(muzs.FindPropertyRelative("Array.size"), soundLabel);
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Clips", MessageType.None);
			GUI.color = backgroundColor;
			for (int i = 0; i < muzs.arraySize; i++)
			{
				GUIContent label = new GUIContent("Clip " + (i + 1).ToString());
				EditorGUILayout.PropertyField(muzs.GetArrayElementAtIndex(i), label);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Settings", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("soundRange"), new GUIContent("Range"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("soundVolume"), new GUIContent("Volume"));
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroSolarPanel : MonoBehaviour
{
	//
	[HideInInspector] public SilantroModule[] seriesModules;
	[HideInInspector] public SilantroModule[] parallelModules;
	public enum ConnectionType
	{
		Series,
		Parallel,
		Combined
	}
	[HideInInspector] public ConnectionType connectionType = ConnectionType.Series;
	[HideInInspector] public float voltage;
	[HideInInspector] public float current;
	[HideInInspector] public float power;

	void Update()
	{
		CalculateModules();
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnDrawGizmos()
	{
		CalculateModules();
		if (seriesModules != null)
		{
			foreach (SilantroModule module in seriesModules)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(module.transform.position, this.transform.position);
			}
		}
		if (parallelModules != null)
		{
			foreach (SilantroModule module in parallelModules)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(module.transform.position, this.transform.position);
			}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void CalculateModules()
	{
		//
		voltage = 0;
		current = 0;
		power = 0;
		//
		if (connectionType == ConnectionType.Combined)
		{
			//
			current = seriesModules[0].terminalCurrent;
			foreach (SilantroModule module in seriesModules)
			{
				voltage += module.terminalVoltage;
			}
			//
			voltage += parallelModules[0].terminalVoltage;
			foreach (SilantroModule module in parallelModules)
			{
				current += module.terminalCurrent;
			}
		}
		if (connectionType == ConnectionType.Parallel)
		{
			//
			voltage = parallelModules[0].terminalVoltage;
			foreach (SilantroModule module in parallelModules)
			{
				current += module.terminalCurrent;
			}
		}
		if (connectionType == ConnectionType.Series)
		{
			//
			current = seriesModules[0].terminalCurrent;
			foreach (SilantroModule module in seriesModules)
			{
				voltage += module.terminalVoltage;
			}
		}
		//Calculate Starting Power
		power = voltage * current;
	}

}
//
#if UNITY_EDITOR
[CustomEditor(typeof(SilantroSolarPanel))]
public class SilantroPanelEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = Color.yellow;
	//

	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		serializedObject.Update();
		SilantroSolarPanel panel = (SilantroSolarPanel)target;



		GUILayout.Space(3f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Module Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("connectionType"), new GUIContent("Module Arrangement"));
		GUILayout.Space(5f);
		if (panel.connectionType == SilantroSolarPanel.ConnectionType.Combined)
		{
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Series Modules", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			GUIContent seriesLabel = new GUIContent("Module Count");
			GUILayout.Space(5f);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();


			SerializedProperty series = this.serializedObject.FindProperty("seriesModules");
			EditorGUILayout.PropertyField(series.FindPropertyRelative("Array.size"), seriesLabel);
			GUILayout.Space(5f);
			for (int i = 0; i < series.arraySize; i++)
			{
				GUIContent label = new GUIContent("Module " + (i + 1).ToString());
				EditorGUILayout.PropertyField(series.GetArrayElementAtIndex(i), label);
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Parallel Modules", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);

			GUILayout.Space(5f);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();

			SerializedProperty parallel = this.serializedObject.FindProperty("parallelModules");
			EditorGUILayout.PropertyField(parallel.FindPropertyRelative("Array.size"), seriesLabel);
			GUILayout.Space(5f);
			for (int i = 0; i < parallel.arraySize; i++)
			{
				GUIContent label = new GUIContent("Module " + (i + 1).ToString());
				EditorGUILayout.PropertyField(parallel.GetArrayElementAtIndex(i), label);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
		if (panel.connectionType == SilantroSolarPanel.ConnectionType.Series)
		{
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Series Modules", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			GUIContent seriesLabel = new GUIContent("Module Count");
			GUILayout.Space(5f);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();

			SerializedProperty series = this.serializedObject.FindProperty("seriesModules");
			EditorGUILayout.PropertyField(series.FindPropertyRelative("Array.size"), seriesLabel);
			GUILayout.Space(5f);
			for (int i = 0; i < series.arraySize; i++)
			{
				GUIContent label = new GUIContent("Module " + (i + 1).ToString());
				EditorGUILayout.PropertyField(series.GetArrayElementAtIndex(i), label);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
		if (panel.connectionType == SilantroSolarPanel.ConnectionType.Parallel)
		{
			//
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Parallel Modules", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);

			GUIContent seriesLabel = new GUIContent("Module Count");
			GUILayout.Space(5f);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			SerializedProperty parallel = this.serializedObject.FindProperty("parallelModules");
			EditorGUILayout.PropertyField(parallel.FindPropertyRelative("Array.size"), seriesLabel);
			GUILayout.Space(5f);
			for (int i = 0; i < parallel.arraySize; i++)
			{
				GUIContent label = new GUIContent("Module " + (i + 1).ToString());
				EditorGUILayout.PropertyField(parallel.GetArrayElementAtIndex(i), label);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Panel Output", MessageType.None);
		GUI.color = backgroundColor;

		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Terminal Voltage", panel.voltage.ToString("0.0") + " Volts");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Terminal Current", panel.current.ToString("0.0") + " Amps");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Effective Power", panel.power.ToString("0.0") + " Watts");

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
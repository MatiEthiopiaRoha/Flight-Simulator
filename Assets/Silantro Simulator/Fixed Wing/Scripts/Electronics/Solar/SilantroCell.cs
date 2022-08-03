using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroCell : MonoBehaviour
{
	//
	//CELL TYPES
	public enum CellType
	{
		MonoCrystalineSilicon,//20%
		PolyCrystallineSilicon,//15%
		ThinFilm,//12%
		AmorphousSilicon,//8%
		CadmiumTellride,//14.4%
		CuIndiumGalliumSelenide,//12%
		ConcentratePV//41%
	}
	[HideInInspector] public CellType cellType = CellType.ConcentratePV;
	//
	//EFFICIENCY DEFINITIONS FOR EACH CELL
	[HideInInspector] public float mono_Si = 20f;
	[HideInInspector] public float P_Si = 15f;
	[HideInInspector] public float TSFC = 12f;
	[HideInInspector] public float a_Si = 8f;
	[HideInInspector] public float CdTe = 14.4f;
	[HideInInspector] public float CIGS = 12f;
	[HideInInspector] public float ConcPVC = 41f;
	//
	//TERMINAL VOLTAGE DEFINITIONS
	[HideInInspector] public float mono_SiVoltage = 0.613f;
	[HideInInspector] public float P_SiVoltage = 0.602f;
	[HideInInspector] public float TSFCVoltage = 0.593f;
	[HideInInspector] public float a_SiVoltage = 0.523f;
	[HideInInspector] public float CdTeVoltage = 0.596f;
	[HideInInspector] public float CIGSVoltage = 0.540f;
	[HideInInspector] public float ConcPVCVoltage = 0.704f;
	//
	//TERMINAL VOLTAGE DEFINITIONS
	[HideInInspector] public float mono_SiCurrent = 2.704f;
	[HideInInspector] public float P_SiCurrent = 2.655f;
	[HideInInspector] public float TSFCCurrent = 2.615f;
	[HideInInspector] public float a_SiCurrent = 2.307f;
	[HideInInspector] public float CdTeCurrent = 2.629f;
	[HideInInspector] public float CIGSCurrent = 2.282f;
	[HideInInspector] public float ConcPVCCurrent = 3.105f;
	//
	[HideInInspector] public float junctionVoltage;
	[HideInInspector] public float junctionCurrent;
	[HideInInspector] public float cellEfficiency;
	//
	[HideInInspector] public float cellLength = 60;
	[HideInInspector] public float cellWidth = 30;
}
//
#if UNITY_EDITOR
[CustomEditor(typeof(SilantroCell))]
public class SilantroCellEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = Color.yellow;
	//

	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector();
		serializedObject.Update();

		SilantroCell cell = (SilantroCell)target;
		GUILayout.Space(3f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Cell Properties", MessageType.None);
		GUI.color = backgroundColor;


		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("cellType"), new GUIContent("Cell Type"));
		if (cell.cellType == SilantroCell.CellType.AmorphousSilicon)
		{
			cell.junctionCurrent = cell.a_SiCurrent;
			cell.junctionVoltage = cell.a_SiVoltage;
			cell.cellEfficiency = cell.a_Si;
		}
		if (cell.cellType == SilantroCell.CellType.CadmiumTellride)
		{
			cell.junctionCurrent = cell.CdTeCurrent;
			cell.junctionVoltage = cell.CdTeVoltage;
			cell.cellEfficiency = cell.CdTe;
		}
		if (cell.cellType == SilantroCell.CellType.ConcentratePV)
		{
			cell.junctionCurrent = cell.ConcPVCCurrent;
			cell.junctionVoltage = cell.ConcPVCVoltage;
			cell.cellEfficiency = cell.ConcPVC;
		}
		if (cell.cellType == SilantroCell.CellType.CuIndiumGalliumSelenide)
		{
			cell.junctionCurrent = cell.CIGSCurrent;
			cell.junctionVoltage = cell.CIGSVoltage;
			cell.cellEfficiency = cell.CIGS;
		}
		if (cell.cellType == SilantroCell.CellType.MonoCrystalineSilicon)
		{
			cell.junctionCurrent = cell.mono_SiCurrent;
			cell.junctionVoltage = cell.mono_SiVoltage;
			cell.cellEfficiency = cell.mono_Si;
		}
		if (cell.cellType == SilantroCell.CellType.PolyCrystallineSilicon)
		{
			cell.junctionCurrent = cell.P_SiCurrent;
			cell.junctionVoltage = cell.P_SiVoltage;
			cell.cellEfficiency = cell.P_Si;
		}
		if (cell.cellType == SilantroCell.CellType.ThinFilm)
		{
			cell.junctionCurrent = cell.TSFCCurrent;
			cell.junctionVoltage = cell.TSFCVoltage;
			cell.cellEfficiency = cell.TSFC;
		}
		GUILayout.Space(5f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Dimensions", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("cellLength"), new GUIContent("Cell Length"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("cellWidth"), new GUIContent("Cell Width"));

		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Junction Output", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Current", cell.junctionCurrent.ToString("0.0") + " A");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Voltage", cell.junctionVoltage.ToString("0.0") + " V");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Efficiency", cell.cellEfficiency.ToString("0.0") + " %");

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public class SilantroPylon : MonoBehaviour
{


	// ------------------------------------------ Selectibles
	public enum PylonPosition { External, Internal }
	public PylonPosition pylonPosition;
	public enum LauncherType { Trapeze, Drop, Tube }
	public LauncherType laucnherType = LauncherType.Drop;
	public enum TrapezePosition { Left, Right, Central }
	public TrapezePosition trapezePosition = TrapezePosition.Right;
	public enum OrdnanceType { Bomb, Missile }
	public OrdnanceType munitionType = OrdnanceType.Bomb;
	public enum DropMode { Single, Salvo }
	public DropMode bombMode = DropMode.Single;
	public enum FireCondition { Normal, Overide }
	public FireCondition fireCondition = FireCondition.Normal;



	// ------------------------------------------ Variables
	public Transform target;
	public string targetID;
	public SilantroArmament manager;
	public SilantroActuator pylonBay;
	public SilantroMunition missile;
	public List<SilantroMunition> bombs;
	public float dropInterval = 1f;
	public float waitTime;



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//COUNT ATTACHED MUNITIONS
	public void InitializePylon()
	{
		//CLOSE DOOR
		if (pylonBay != null && pylonPosition == PylonPosition.Internal)
		{
			if (pylonBay.actuatorMode == SilantroActuator.ActuatorMode.DefaultOpen && pylonBay.actuatorState == SilantroActuator.ActuatorState.Engaged)
			{
				pylonBay.DisengageActuator();
			}
		}
		//IDENTIFY ATTACHED MUNITION
		SilantroMunition[] munitions = GetComponentsInChildren<SilantroMunition>();
		bombs = new List<SilantroMunition>();
		foreach (SilantroMunition munition in munitions)
		{
			//MISSILE
			if (munitionType == OrdnanceType.Missile)
			{
				if (munition.munitionType == SilantroMunition.MunitionType.Missile)
				{
					missile = munition;
					missile.connectedPylon = this.gameObject.GetComponent<SilantroPylon>();
				}
			}
			//BOMB
			if (munitionType == OrdnanceType.Bomb)
			{
				if (munition.munitionType == SilantroMunition.MunitionType.Bomb)
				{
					bombs.Add(munition);
					munition.connectedPylon = this.gameObject.GetComponent<SilantroPylon>();
				}
			}
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//RECOUNT BOMBS
	void CountBombs()
	{
		SilantroMunition[] munitions = GetComponentsInChildren<SilantroMunition>();
		bombs = new List<SilantroMunition>();
		foreach (SilantroMunition munition in munitions)
		{
			//BOMB
			if (munitionType == OrdnanceType.Bomb)
			{
				if (munition.munitionType == SilantroMunition.MunitionType.Bomb)
				{
					bombs.Add(munition);
				}
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//START SEQUENCE LAUNCH
	public void StartLaunchSequence()
	{
		engaged = true;
		//DETERMINE LAUNCH SEQUENCE
		if (pylonPosition == PylonPosition.External)
		{
			LaunchMissile();
		}
		if (pylonPosition == PylonPosition.Internal)
		{
			//OPEN DOOR
			if (pylonBay != null)
			{
				StartCoroutine(OpenBayDoor());
			}
			//LAUNCH IF DOOR IS UNAVAILABLE
			else
			{
				LaunchMissile();
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//START SEQUENCE DROP
	public void StartDropSequence()
	{
		if (pylonPosition == PylonPosition.External)
		{
			BombRelease();
		}
		if (pylonPosition == PylonPosition.Internal)
		{
			//OPEN DOOR
			engaged = true;
			if (pylonBay != null)
			{
				bombMode = DropMode.Salvo;
				StartCoroutine(OpenBayDoor());
			}
			//LAUNCH IF DOOR IS UNAVAILABLE
			else
			{
				BombRelease();
			}
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public bool engaged;
	//OPEN DOOR
	IEnumerator OpenBayDoor()
	{
		if (pylonBay.actuatorState == SilantroActuator.ActuatorState.Disengaged) { pylonBay.EngageActuator(); }

		yield return new WaitUntil(() => pylonBay.actuatorState == SilantroActuator.ActuatorState.Engaged);
		//RELEASE MUNITION
		if (munitionType == OrdnanceType.Missile) { LaunchMissile(); }
		if (munitionType == OrdnanceType.Bomb) { BombRelease(); }
	}


	//CLOSE DOOR
	IEnumerator CloseDoor()
	{
		yield return new WaitForSeconds(0.5f);
		if (pylonBay.actuatorState == SilantroActuator.ActuatorState.Engaged) { pylonBay.DisengageActuator(); }
		//REMOVE PYLON
		Destroy(this.gameObject);
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ACTUAL MISSILE LAUNCH
	void LaunchMissile()
	{
		//1. TUBE LAUNCH
		if (laucnherType == LauncherType.Tube)
		{
			missile.FireMunition(target, targetID, 2);
		}
		//2. DROP LAUNCH
		if (laucnherType == LauncherType.Drop)
		{
			missile.FireMunition(target, targetID, 1);
		}

		//3. TRAPEZE LAUNCH RIGHT
		if (laucnherType == LauncherType.Trapeze && trapezePosition == TrapezePosition.Right)
		{
			missile.FireMunition(target, targetID, 3);
		}

		//4. TRAPEZE LAUNCH LEFT
		if (laucnherType == LauncherType.Trapeze && trapezePosition == TrapezePosition.Left)
		{
			missile.FireMunition(target, targetID, 4);
		}

		//5. TRAPEZE LAUNCH MIDDLE
		if (laucnherType == LauncherType.Trapeze && trapezePosition == TrapezePosition.Central)
		{
			missile.FireMunition(target, targetID, 5);
		}

		//CLOSE BAY DOOR
		if (pylonPosition == PylonPosition.Internal && pylonBay != null)
		{
			StartCoroutine(CloseDoor());
		}

		manager.CountOrdnance();
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ACTUAL BOMB DROP
	void BombRelease()
	{
		//1. SINGLE BOMB DROP
		//SELECT RANDOM BOMB
		if (bombs.Count > 0)
		{
			if (bombs[0] != null)
			{
				bombs[0].ReleaseMunition();
				manager.CountOrdnance();
				CountBombs();
			}
			
		

			//2. SALVO DROP
			if (bombMode == DropMode.Salvo)
			{
				StartCoroutine(WaitForNextDrop());
			}
		}
		else
		{
			if (pylonPosition == PylonPosition.Internal && pylonBay != null && pylonBay.actuatorState == SilantroActuator.ActuatorState.Engaged)
			{
				StartCoroutine(CloseDoor());
			}
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SALVO TIMER
	IEnumerator WaitForNextDrop()
	{
		yield return new WaitForSeconds(dropInterval);
		BombRelease();
		manager.CountOrdnance();
		CountBombs();
	}
}





#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroPylon))]
public class SilantroPylonEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);
	SilantroPylon pylon;


	private void OnEnable()
	{
		pylon = (SilantroPylon)target;
	}

	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		serializedObject.Update();

		GUILayout.Space(1f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Pylon Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("pylonPosition"), new GUIContent("Position"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionType"), new GUIContent("Ordnance"));

		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Launch Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		if (pylon.pylonPosition == SilantroPylon.PylonPosition.Internal)
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("pylonBay"), new GUIContent("Bay Actuator"));
		}
		if (pylon.munitionType == SilantroPylon.OrdnanceType.Bomb)
		{
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("bombMode"), new GUIContent("Drop Mode"));

			if (pylon.bombMode == SilantroPylon.DropMode.Salvo)
			{
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("dropInterval"), new GUIContent("Drop Interval"));
			}
		}
		if (pylon.munitionType == SilantroPylon.OrdnanceType.Missile)
		{
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("laucnherType"), new GUIContent("Launch Mode"));

			if (pylon.laucnherType == SilantroPylon.LauncherType.Trapeze)
			{
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("trapezePosition"), new GUIContent("Launch Position"));
			}
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
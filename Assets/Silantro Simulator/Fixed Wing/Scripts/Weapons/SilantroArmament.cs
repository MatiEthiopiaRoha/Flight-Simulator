using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif



/// <summary>
///
/// 
/// Use:		 Handles the processing and firing of the attached weapons
/// </summary>


public class SilantroArmament : MonoBehaviour
{
	//1. --------------------------------------------------------------Rockets
	public List<SilantroMunition> rockets;
	public float rateOfFire = 3f;
	public float actualFireRate;
	float fireTimer;


	//2. --------------------------------------------------------------Missiles
	public List<SilantroMunition> missiles;
	public List<SilantroMunition> AAMS;//AIR TO AIR MISSILES
	public List<SilantroMunition> AGMS;//AIR TO GROUND MISSILES

	public AudioClip fireSound;
	public float fireVolume = 0.7f;
	AudioSource launcherSound;
	public SilantroPylon[] attachedPylons;


	//3. --------------------------------------------------------------Bombs
	public List<SilantroMunition> bombs;//BOMBS
	public List<SilantroPylon> externalBombRacks;
	public List<SilantroPylon> internalBombRacks;
	public float minimumDropHeight = 300f;


	public List<string> availableWeapons = new List<string>();
	public string currentWeapon;
	public int selectedWeapon;
	public bool setRocket;
	public bool setMissile;
	public bool setBomb;


	//4. --------------------------------------------------------------Guns
	public SilantroGun[] attachedGuns;

	public SilantroMunition[] munitions;
	public float weaponsLoad;//Total Weight
	public float viewDirection;
	public SilantroController controller;
	public SilantroRadar connectedRadar;
	public bool canFire;
	public bool canLaunch;
	public Vector3 impactPosition;




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//1. CHANGE SELECTED WEAPON
	public void ChangeWeapon()
	{
		if (controller.isControllable)
		{
			selectedWeapon += 1;
			if (selectedWeapon > (availableWeapons.Count - 1)) { selectedWeapon = 0; }
			currentWeapon = availableWeapons[selectedWeapon];
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//2. SELECT WEAPON
	public void SelectWeapon(int weaponPoint)
	{
		currentWeapon = availableWeapons[weaponPoint];
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//INITIALIZE Armament
	public void InitializeWeapons()
	{
		//0.
		if (connectedRadar == null)
		{
			Debug.Log("No Radar is connected to the aircraft, some functionalities will not work");
			return;
		}

		//1. SOUND
		if (fireSound) { Oyedoyin.Handler.SetupSoundSource(this.transform, fireSound, "Launch Sound Point", 100f, false, false, out launcherSound); launcherSound.volume = fireVolume; }


		//ROCKET FIRE RATE
		if (rateOfFire != 0) { actualFireRate = 1.0f / rateOfFire; }
		else { actualFireRate = 0.01f; }
		fireTimer = 0.0f;
		Rigidbody connectedAircraft = connectedRadar.connectedAircraft.GetComponent<Rigidbody>();

		SilantroMunition[] munitions = connectedAircraft.gameObject.GetComponentsInChildren<SilantroMunition>();
		foreach (SilantroMunition munition in munitions)
		{
			if (connectedAircraft != null) { munition.connectedAircraft = connectedAircraft; }
			if (controller != null) { munition.controller = controller; }
			munition.InitializeMunition();
		}
		//SETUP GUNS
		attachedGuns = connectedAircraft.gameObject.GetComponentsInChildren<SilantroGun>();
		foreach (SilantroGun gun in attachedGuns)
		{
			if (connectedRadar != null) { gun.controller = controller; }
			gun.InitializeGun();
		}
		//REFRESH PYLON INFO
		attachedPylons = connectedRadar.connectedAircraft.gameObject.GetComponentsInChildren<SilantroPylon>();
		foreach (SilantroPylon pylon in attachedPylons)
		{
			pylon.manager = this.gameObject.GetComponent<SilantroArmament>();
			pylon.InitializePylon();
		}


		//INITIAL COUNT
		CountOrdnance();
		//SET AVAILABLE WEAPONS
		if (attachedGuns.Length > 0) { availableWeapons.Add("Gun"); }
		if (bombs.Count > 0) { availableWeapons.Add("Bomb"); }
		if (missiles.Count > 0) { availableWeapons.Add("Missile"); }
		if (rockets.Count > 0) { availableWeapons.Add("Rocket"); }


		//SELECT DEFAULT WEAPON
		selectedWeapon = 0;
		if (availableWeapons.Count > 0)
		{
			currentWeapon = availableWeapons[selectedWeapon];
			//SELECT TARGET FILTER
			if (connectedRadar != null)
			{
				//if(currentWeapon == "Gun"){connectedRadar.objectFilter = SilantroRadar.ObjectFilter.Ground;}
				//if(currentWeapon == "Missile"){connectedRadar.objectFilter = SilantroRadar.ObjectFilter.Air;}
			}
		}
		else
		{
			Debug.Log("No weapons attached to Fire-Control System!!");
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//COUNT ROCKETS
	public void CountOrdnance()
	{
		munitions = connectedRadar.connectedAircraft.gameObject.GetComponentsInChildren<SilantroMunition>();
		//RESET BUCKETS
		weaponsLoad = 0f;
		rockets.Clear();
		missiles.Clear();
		bombs.Clear();
		AAMS.Clear();
		AGMS.Clear();

		//COUNT
		foreach (SilantroMunition munition in munitions)
		{
			weaponsLoad += munition.munitionWeight;
			//SEPARATE ROCKETS
			if (munition.munitionType == SilantroMunition.MunitionType.Rocket)
			{
				rockets.Add(munition);
			}
			//SEPARATE MISSILE
			if (munition.munitionType == SilantroMunition.MunitionType.Missile)
			{
				//CHECK IF LAUNCH SEQUENCE HAS BEEN INITIALIZED
				if (munition.connectedPylon != null && munition.connectedPylon.engaged != true)
				{
					missiles.Add(munition);
					//BY TYPE
					if (munition.missileType == SilantroMunition.MissileType.AAM) { AAMS.Add(munition); }
					if (munition.missileType == SilantroMunition.MissileType.ASM) { AGMS.Add(munition); }
				}
			}
			//SEPARATE BOMB
			if (munition.munitionType == SilantroMunition.MunitionType.Bomb)
			{
				bombs.Add(munition);
			}
		}
		//REFRESH BOMB PYLONS
		externalBombRacks = new List<SilantroPylon>(); internalBombRacks = new List<SilantroPylon>();
		attachedPylons = GetComponentsInChildren<SilantroPylon>();
		foreach (SilantroPylon pylon in attachedPylons)
		{
			if (pylon.bombs.Count > 0)
			{
				//1. EXTERNAL PYLON
				if (pylon.munitionType == SilantroPylon.OrdnanceType.Bomb && pylon.pylonPosition == SilantroPylon.PylonPosition.External)
				{
					externalBombRacks.Add(pylon);
				}
				//2. INTERNAL BAY
				if (pylon.munitionType == SilantroPylon.OrdnanceType.Bomb && pylon.pylonPosition == SilantroPylon.PylonPosition.Internal)
				{
					internalBombRacks.Add(pylon);
				}
			}
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//1. ROCKET
	public void FireRocket()
	{
		if (controller != null && controller.isControllable)
		{
			if (rockets.Count > 0)
			{
				fireTimer = 0f;
				//SELECT RANDOM ROCKET
				int index = UnityEngine.Random.Range(0, rockets.Count);
				if (rockets[index] != null)
				{
					//FIRE GUIDED ROCKET
					if (rockets[index].rocketType == SilantroMunition.RocketType.Guided)
					{
						if (connectedRadar != null && connectedRadar.lockedTarget != null)
						{
							//COLLECT PROPERTIES
							Transform lockedTarget = connectedRadar.lockedTarget.body.transform;
							string lockedTargetID = connectedRadar.lockedTarget.trackingID;
							rockets[index].FireMunition(lockedTarget, lockedTargetID, 0);
						}
						else
						{
							rockets[index].ReleaseMunition();
						}
					}
					//LAUNCH UNGUIDED ROCKET
					if (rockets[index].rocketType == SilantroMunition.RocketType.Unguided)
					{
						rockets[index].ReleaseMunition();
					}
					//PLAY SOUND
					launcherSound.PlayOneShot(fireSound);
					CountOrdnance();
				}
			}
			else
			{
				Debug.Log("Rocket System Offline");
			}
		}
		else
		{
			Debug.Log("Weapon System Offline");
		}
	}


	//2. GUNS
	public void FireGuns()
	{
		if (controller != null && controller.isControllable)
		{
			if (attachedGuns.Length > 0)
			{
				foreach (SilantroGun gun in attachedGuns)
				{
					gun.FireGun();
				}
			}
			//NO GUNS
			else
			{
				Debug.Log("Gun System Offline");
			}
		}
		//NOT CONTROLLABLE
		else
		{
			Debug.Log("Weapon System Offline");
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//3. BOMB
	public void DropBomb()
	{
		if (controller != null && controller.isControllable)
		{
			if (connectedRadar.connectedAircraft.transform.position.y > minimumDropHeight)
			{
				if (bombs.Count > 0)
				{
					if (externalBombRacks.Count > 0)
					{
						//SELECT RANDOM BOMB FROM EXTERNAL RACK
						if (externalBombRacks[0].bombs.Count > 0 && !externalBombRacks[0].engaged)
						{
							externalBombRacks[0].StartDropSequence();
						}
					}
					else if (internalBombRacks.Count > 0)
					{
						//SELECT RANDOM BOMB FROM INTERNAL RACK
						if (internalBombRacks[0].bombs.Count > 0 && !internalBombRacks[0].engaged)
						{
							internalBombRacks[0].StartDropSequence();
						}
					}
					CountOrdnance();
				}
				//NO BOMBS
				else
				{
					Debug.Log("Bomb System Offline");
				}
			}
			//TOO LOW
			else
			{
				Debug.Log("Aircraft too low to drop bombs");
			}
		}
		else
		{
			Debug.Log("Weapon System Offline");
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//4. MISSILE
	public void FireMissile()
	{
		if (controller != null && controller.isControllable)
		{
			CountOrdnance();
			//MAKE SURE MISSILES ARE AVAILABLE
			if (missiles.Count > 0)
			{
				//COLLECT TARGET DATA
				if (connectedRadar != null && connectedRadar.lockedTarget != null)
				{
					//CHECK DIRECTION
					if (viewDirection < 0.6f)
					{
						Debug.Log("Missile launch cancelled. Reason: Target out of view range");
					}
					else
					{

						//1. PROCESS AIR TARGET
						if (connectedRadar.lockedTarget.form == "Aircraft")
						{
							//SELECT AAM
							if (AAMS.Count > 0)
							{
								//SELECT RANDOM AAM
								int index = UnityEngine.Random.Range(0, AAMS.Count);
								if (AAMS[index] != null && AAMS[index].connectedPylon != null && !AAMS[index].connectedPylon.engaged)
								{
									AAMS[index].connectedPylon.target = connectedRadar.lockedTarget.body.transform;//SET TARGET
									AAMS[index].computer.supportRadar = connectedRadar;//SET SUPPORT RADAR ?? FOR SEMI_ACTIVE GUIDANCE
									AAMS[index].connectedPylon.targetID = connectedRadar.lockedTarget.trackingID;//SET TARGET ID
									AAMS[index].connectedPylon.StartLaunchSequence();//TRIGGER
								}
							}
							else if (AGMS.Count > 0)
							{
								Debug.Log("No AAM Available so AGM has been launched");
								//SELECT RANDOM AGM
								int index = UnityEngine.Random.Range(0, AGMS.Count);
								if (AGMS[index] != null && AGMS[index].connectedPylon != null && !AGMS[index].connectedPylon.engaged)
								{
									AGMS[index].connectedPylon.target = connectedRadar.lockedTarget.body.transform;
									AGMS[index].computer.supportRadar = connectedRadar;
									AGMS[index].connectedPylon.targetID = connectedRadar.lockedTarget.trackingID;
									AGMS[index].connectedPylon.StartLaunchSequence();
								}
							}
							//PLAY SOUND
							launcherSound.PlayOneShot(fireSound);
							CountOrdnance();
						}

						//2. PROCESS GROUND TARGET
						else if (connectedRadar.lockedTarget.form == "SAM Battery" || connectedRadar.lockedTarget.form == "Truck" || connectedRadar.lockedTarget.form == "Tank")
						{
							//SELECT AGM
							if (AGMS.Count > 0)
							{
								//SELECT RANDOM AGM
								int index = UnityEngine.Random.Range(0, AGMS.Count);
								if (AGMS[index] != null && AGMS[index].connectedPylon != null && !AGMS[index].connectedPylon.engaged)
								{
									AGMS[index].connectedPylon.target = connectedRadar.lockedTarget.body.transform;
									AGMS[index].computer.supportRadar = connectedRadar;
									AGMS[index].connectedPylon.targetID = connectedRadar.lockedTarget.trackingID;
									AGMS[index].connectedPylon.StartLaunchSequence();
								}
							}
							else if (AAMS.Count > 0)
							{
								Debug.Log("No AGM Available so AAM has been launched");
								//SELECT RANDOM AAM
								int index = UnityEngine.Random.Range(0, AAMS.Count);
								if (AAMS[index] != null && AAMS[index].connectedPylon != null && !AAMS[index].connectedPylon.engaged)
								{
									AAMS[index].connectedPylon.target = connectedRadar.lockedTarget.body.transform;
									AAMS[index].computer.supportRadar = connectedRadar;
									AAMS[index].connectedPylon.targetID = connectedRadar.lockedTarget.trackingID;
									AAMS[index].connectedPylon.StartLaunchSequence();
								}
							}
							//PLAY SOUND
							launcherSound.PlayOneShot(fireSound);
							CountOrdnance();
						}


						//3.
						else
						{
							Debug.Log("Locked Target form is either null or not supported. You can add a new definition in the Armaments code");
						}
					}
				}
				//NO LOCKED TARGET
				else
				{
					Debug.Log("Locked Target/Radar Unavailable");
				}
			}
			//NO MISSILES
			else
			{
				Debug.Log("Missile System Offline");
			}
		}
		else
		{
			Debug.Log("Weapon System Offline");
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//REFRESH
	void Update()
	{
		if (controller != null && controller.isControllable)
		{
			fireTimer += Time.deltaTime;
			if (connectedRadar != null && connectedRadar.lockedTarget != null && connectedRadar.lockedTarget.body != null)
			{
				Vector3 targetDirection = (connectedRadar.lockedTarget.body.transform.position - connectedRadar.transform.position).normalized;
				viewDirection = Vector3.Dot(targetDirection, connectedRadar.transform.forward);
			}

			if (controller.isControllable && bombs != null && bombs.Count > 0)
			{
				//PREDICT BOMB IMPACT POSITION
				if (connectedRadar && connectedRadar.connectedAircraft)
				{
					CalculateImpactPosition();
				}
			}
		}
	}



	void CalculateImpactPosition()
	{
		float altitude = this.transform.position.y;
		float time = Mathf.Pow(((altitude * 2f) / 9.8f), 0.5f);

		float speed = connectedRadar.connectedAircraft.GetComponent<Rigidbody>().velocity.magnitude;
		float distanceInitial = time * speed;
		Vector3 direction = transform.forward;
		Vector3 finalDirection = direction.normalized * distanceInitial;
		Vector3 finalPosition = this.transform.position + finalDirection;
		impactPosition = new Vector3(finalPosition.x, 0, finalPosition.z);

		Debug.DrawLine(this.transform.position, impactPosition);
		//		if (actualtarget != null) {
		//			actualtarget.transform.position = impactPosition + new Vector3(0,2,0);
		//		}
	}
}







#if UNITY_EDITOR
[CustomEditor(typeof(SilantroArmament))]
public class SilantroArmamentEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);
	SilantroArmament carrier;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnEnable()
	{
		carrier = (SilantroArmament)target;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector();
		serializedObject.Update();



		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("System Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Payload", carrier.weaponsLoad.ToString("0.0") + " kg");
		int mun = 0;
		if (carrier.munitions != null) { mun = carrier.munitions.Length; }
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Munition Count", mun.ToString());
		GUILayout.Space(8f);
		if (!carrier.setMissile)
		{
			if (GUILayout.Button("Configure Missile"))
			{
				carrier.setMissile = true;
			}
		}
		if (carrier.setMissile)
		{
			if (GUILayout.Button("Hide Missile Board"))
			{
				carrier.setMissile = false;
			}
			GUILayout.Space(8f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Missile Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			int count = 0;
			if (carrier.missiles != null) { count = carrier.missiles.Count; }
			EditorGUILayout.LabelField("Missile Count", count.ToString());
			if (carrier.AAMS != null)
			{
				GUILayout.Space(2f);
				EditorGUILayout.LabelField("AAM Count", carrier.AAMS.Count.ToString());
			}
			if (carrier.AGMS != null)
			{
				GUILayout.Space(2f);
				EditorGUILayout.LabelField("AGM Count", carrier.AGMS.Count.ToString());
			}

			GUILayout.Space(8f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Launcher Sound", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("fireSound"), new GUIContent("Fire Sound"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("fireVolume"), new GUIContent("Fire Volume"));
		}

		//2. ROCKETS
		GUILayout.Space(10f);
		if (!carrier.setRocket)
		{
			if (GUILayout.Button("Configure Rockets"))
			{
				carrier.setRocket = true;
			}
		}
		if (carrier.setRocket)
		{
			if (GUILayout.Button("Hide Rocket Board"))
			{
				carrier.setRocket = false;
			}
			GUILayout.Space(8f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Rocket Configuration", MessageType.None);
			GUI.color = backgroundColor;
			if (carrier.rockets != null)
			{
				GUILayout.Space(2f);
				EditorGUILayout.LabelField("Rocket Count", carrier.rockets.Count.ToString());
			}
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rateOfFire"), new GUIContent("Rate of Fire"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("AROF", carrier.actualFireRate.ToString("0.00"));
		}

		//2. BOMB
		GUILayout.Space(10f);
		if (!carrier.setBomb)
		{
			if (GUILayout.Button("Configure Bombs"))
			{
				carrier.setBomb = true;
			}
		}
		if (carrier.setBomb)
		{
			if (GUILayout.Button("Hide Bomb Board"))
			{
				carrier.setBomb = false;
			}
			GUILayout.Space(8f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Bomb Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumDropHeight"), new GUIContent("Minimum Drop Height"));
			if (carrier.bombs != null)
			{
				GUILayout.Space(2f);
				EditorGUILayout.LabelField("Bomb Count", carrier.bombs.Count.ToString());
			}
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("External Racks", carrier.externalBombRacks.Count.ToString());
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Internal Racks", carrier.internalBombRacks.Count.ToString());
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
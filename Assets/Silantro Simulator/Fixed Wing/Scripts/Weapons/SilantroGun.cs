using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oyedoyin;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif




public class SilantroGun : MonoBehaviour
{
	// ----------------------------------------------- Selectibles
	public enum WeaponType { Machine, Cannon }
	public WeaponType weaponType = WeaponType.Machine;
	public enum BulletType { Raycast, Rigidbody }
	public BulletType bulletType;
	public enum RotationAxis { X, Y, Z }
	public RotationAxis rotationAxis = RotationAxis.X;
	public enum RotationDirection { CW, CCW }
	public RotationDirection rotationDirection = RotationDirection.CCW;



	// ----------------------------------------------- Variables
	public float rateOfFire = 500;
	public float actualRate;
	public float fireTimer;
	public float accuracy = 80f;
	public float currentAccuracy;
	public float accuracyDrop = 0.2f;
	public float accuracyRecover = 0.5f;
	float acc;

	public float muzzleVelocity = 500;
	public float barrelLength = 2f;
	public float gunWeight;
	public float drumWeight;
	public float damage;
	private float barrelRPM;
	public float currentRPM;
	public Vector3 baseVelocity;


	public float projectileForce;
	public float damperStrength = 90f;
	public int ammoCapacity = 1000;
	public int currentAmmo;
	public bool unlimitedAmmo;
	private int muzzle = 0;
	public bool advancedSettings;
	public float range = 1000f;
	public float rangeRatio = 1f;


	private float shellSpitForce = 1.5f;
	private float shellForceRandom = 1.5f;
	private float shellSpitTorqueX = 0.5f;
	private float shellSpitTorqueY = 0.5f;
	private float shellTorqueRandom = 1.0f;
	public bool ejectShells = false;
	public bool canFire = true;
	public bool running;



	// ----------------------------------------------- Connections
	public SilantroController controller;
	public GameObject ammunition;
	GameObject currentBullet;
	public Transform barrel;
	public GameObject bulletCase;
	public Transform shellEjectPoint;
	public Transform[] muzzles;
	private Transform currentMuzzle;


	// ----------------------------------------------- Sounds
	public AudioClip fireLoopSound;
	public AudioClip fireEndSound;
	public float soundVolume = 0.75f;
	public AudioSource gunLoopSource, gunEndSource;
	float bulletMass;
	public float soundRange = 150f;


	// ---------------------------------------------- Effects
	public GameObject muzzleFlash;
	public GameObject groundHit;
	public GameObject metalHit;
	public GameObject woodHit;//ADD MORE



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//FIRE FUNCTION
	public void FireGun()
	{
		if (canFire) { if ((fireTimer > (actualRate))) { Fire(); } }
		//OFFLINE
		else { Debug.Log("Gun System Offline"); }
	}





	bool allOk;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	protected void _checkPrerequisites()
	{
		//CHECK COMPONENTS
		if (controller != null && fireLoopSound != null && fireEndSound != null)
		{
			allOk = true;
		}
		else if (controller == null)
		{
			Debug.LogError("Prerequisites not met on " + transform.name + "....Aircraft rigidbody not assigned");
			allOk = false;
		}
		else if (fireEndSound == null)
		{
			Debug.LogError("Prerequisites not met on " + transform.name + "....fire end clip not assigned");
			allOk = false;
		}
		else if (fireLoopSound == null)
		{
			Debug.LogError("Prerequisites not met on " + transform.name + "....fire loop clip not assigned");
			allOk = false;
		}
	}







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeGun()
	{


		//----------------------------
		_checkPrerequisites();

		
		if (allOk)
		{
			//SETUP FIRE RATE
			if (rateOfFire > 0)
			{
				float secFireRate = rateOfFire / 60f;//FROM RPM TO RPS
				actualRate = 1.0f / secFireRate;
			}
			else { actualRate = 0.01f; }
			fireTimer = 0.0f;

			// -------------------------------------------------- Base
			currentAmmo = ammoCapacity; barrelRPM = rateOfFire; currentAccuracy = accuracy;
			CountBullets();


			if (fireLoopSound) { Handler.SetupSoundSource(this.transform, fireLoopSound, "Loop Sound Point", soundRange, true, false, out gunLoopSource); gunLoopSource.volume = soundVolume; }
			if (fireEndSound) { Handler.SetupSoundSource(this.transform, fireEndSound, "End Sound Point", soundRange, false, false, out gunEndSource); gunEndSource.volume = soundVolume; }
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void CountBullets()
	{
		// --------------------------------------------------CALCULATE BULLET DRUM WEIGHT
		if (bulletType == BulletType.Rigidbody)
		{
			if (ammunition != null && ammunition.GetComponent<SilantroMunition>() != null)
			{
				bulletMass = ammunition.GetComponent<SilantroMunition>().mass;
			}
			if (ammunition == null) { Debug.Log("Gun " + transform.name + " ammunition gameobject has not been assigned"); return; }
			if (ammunition.GetComponent<SilantroMunition>() == null) { Debug.Log("Gun " + transform.name + " bullet gameobject is invalid, use the prefabs in the Prefabs/Sample/Ammunition/Bullets folder"); }
		}
		else
		{
			if (weaponType == WeaponType.Cannon) { bulletMass = 300f; }
			else { bulletMass = 150f; }
		}
		drumWeight = currentAmmo * ((bulletMass * 0.0648f) / 1000f);
		if (currentAmmo > 0) { canFire = true; }
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SOUND
	void Update()
	{
		if (allOk)
		{
			if (running && currentAmmo <= 0) { gunLoopSource.Stop(); gunEndSource.PlayOneShot(fireEndSound); running = false; }

			if (fireLoopSound != null && fireEndSound != null && canFire)
			{
				if (running && gunLoopSource != null && !gunLoopSource.isPlaying) { gunLoopSource.Play(); }
				if (!running && fireLoopSound != null && gunLoopSource.isPlaying) { gunLoopSource.Stop(); gunEndSource.PlayOneShot(fireEndSound); }
			}
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//REFRESH
	void LateUpdate()
	{
		fireTimer += Time.deltaTime;
		//CLAMP RPM
		if (currentRPM <= 0f) { currentRPM = 0f; }
		//LERP ACCURACY
		currentAccuracy = Mathf.Lerp(currentAccuracy, accuracy, accuracyRecover * Time.deltaTime);
		//CLAMP AMMO
		if (currentAmmo < 0) { currentAmmo = 0; }
		if (currentAmmo == 0) { canFire = false; }
		//CLAMP ROTATION
		if (currentRPM < 0) { currentRPM = 0; }
		if (currentRPM > barrelRPM) { currentRPM = barrelRPM; }

		//ROTATE BARREL
		if (barrel)
		{
			//ANTICLOCKWISE
			if (rotationDirection == RotationDirection.CCW)
			{
				if (rotationAxis == RotationAxis.X) { barrel.Rotate(new Vector3(currentRPM * Time.deltaTime, 0, 0)); }
				if (rotationAxis == RotationAxis.Y) { barrel.Rotate(new Vector3(0, currentRPM * Time.deltaTime, 0)); }
				if (rotationAxis == RotationAxis.Z) { barrel.Rotate(new Vector3(0, 0, currentRPM * Time.deltaTime)); }
			}
			//CLOCKWISE
			if (rotationDirection == RotationDirection.CW)
			{
				if (rotationAxis == RotationAxis.X) { barrel.Rotate(new Vector3(-1f * currentRPM * Time.deltaTime, 0, 0)); }
				if (rotationAxis == RotationAxis.Y) { barrel.Rotate(new Vector3(0, -1f * currentRPM * Time.deltaTime, 0)); }
				if (rotationAxis == RotationAxis.Z) { barrel.Rotate(new Vector3(0, 0, -1f * currentRPM * Time.deltaTime)); }
			}
		}
		//
		//REV GUN UP AND DOWN
		if (running)
		{
			currentRPM = Mathf.Lerp(currentRPM, barrelRPM, Time.deltaTime * 0.5f);
		}
		else
		{
			currentRPM = Mathf.Lerp(currentRPM, 0f, Time.deltaTime * 0.5f);
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ACTUAL FIRE
	void Fire()
	{
		//MAKE SURE THEIR IS A BARREL TO FIRE FROM
		if (muzzles.Length > 0)
		{
			// --------------------------------------------------SELECT A MUZZLE
			muzzle += 1;
			if (muzzle > (muzzles.Length - 1)) { muzzle = 0; }
			currentMuzzle = muzzles[muzzle]; fireTimer = 0f;
			if (controller.aircraft != null) { baseVelocity = controller.aircraft.velocity; }

			// --------------------------------------------------REDUCE AMMO COUNT
			if (!unlimitedAmmo) { currentAmmo--; }
			CountBullets();

			// --------------------------------------------------FIRE DIRECTION AND ACCURACY
			Vector3 direction = currentMuzzle.forward;
			Ray rayout = new Ray(currentMuzzle.position, direction);
			RaycastHit hitout;
			if (Physics.Raycast(rayout, out hitout, range / rangeRatio)) { acc = 1 - ((hitout.distance) / (range / rangeRatio)); }
			// --------------------------------------------------VARY ACCURACY
			float accuracyVary = (100 - currentAccuracy) / 1000;
			direction.x += UnityEngine.Random.Range(-accuracyVary, accuracyVary);
			direction.y += UnityEngine.Random.Range(-accuracyVary, accuracyVary);
			direction.z += UnityEngine.Random.Range(-accuracyVary, accuracyVary);
			currentAccuracy -= accuracyDrop;
			if (currentAccuracy <= 0.0f) currentAccuracy = 0.0f;
			//
			Quaternion muzzleRotation = Quaternion.LookRotation(direction);

			//1. FIRE RIGIDBODY AMMUNITION
			if (bulletType == BulletType.Rigidbody)
			{
				//SHOOT RIGIDBODY
				currentBullet = Instantiate(ammunition, currentMuzzle.position, muzzleRotation) as GameObject;
				SilantroMunition munition = currentBullet.GetComponent<SilantroMunition>();
				if (munition != null)
				{
					munition.controller = controller;
					munition.InitializeMunition();
					munition.ejectionPoint = this.transform.position;
					munition.FireBullet(muzzleVelocity, baseVelocity);
					munition.woodHit = woodHit;
					munition.metalHit = metalHit;
					munition.groundHit = groundHit;
				}
			}

			//2. FIRE RAYCAST AMMUNITION
			if (bulletType == BulletType.Raycast)
			{
				//SETUP RAYCAST
				Ray ray = new Ray(currentMuzzle.position, direction);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, range / rangeRatio))
				{
					//DAMAGE
					float damageeffect = damage * acc;
					hit.collider.gameObject.SendMessage("SilantroDamage", -damageeffect, SendMessageOptions.DontRequireReceiver);
					//INSTANTIATE EFFECTS
					if (hit.collider.CompareTag("Ground") && groundHit != null) { Instantiate(groundHit, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)); }
					//METAL
					if (hit.collider.CompareTag("Metal") && metalHit != null) { Instantiate(metalHit, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)); }
					//WOOD
					if (hit.collider.CompareTag("Wood") && woodHit != null) { Instantiate(woodHit, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)); }
				}
			}

			// --------------------------------------------------RECOIL
			if (controller != null)
			{
				//SET BULLET WEIGHT
				float bulletWeight;
				if (bulletType == BulletType.Rigidbody)
				{
					bulletWeight = currentBullet.GetComponent<SilantroMunition>().mass;
				}
				else
				{
					if (weaponType == WeaponType.Cannon) { bulletWeight = 300f; }
					else { bulletWeight = 150f; }
				}
				float ballisticEnergy = 0.5f * ((bulletWeight * 0.0648f) / 1000f) * muzzleVelocity * muzzleVelocity * UnityEngine.Random.Range(0.9f, 1f);
				projectileForce = ballisticEnergy / barrelLength;

				//APPLY
				Vector3 recoilForce = controller.transform.forward * (-projectileForce * (1 - (damperStrength / 100f)));
				controller.aircraft.AddForce(recoilForce, ForceMode.Impulse);
			}

			// --------------------------------------------------MUZZLE FLASH
			if (muzzleFlash != null)
			{
				GameObject flash = Instantiate(muzzleFlash, currentMuzzle.position, currentMuzzle.rotation);
				flash.transform.position = currentMuzzle.position; flash.transform.parent = currentMuzzle.transform;
			}

			// --------------------------------------------------SHELLS
			if (ejectShells && bulletCase != null)
			{
				GameObject shellGO = Instantiate(bulletCase, shellEjectPoint.position, shellEjectPoint.rotation) as GameObject;
				shellGO.GetComponent<Rigidbody>().velocity = baseVelocity;
				shellGO.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(shellSpitForce + UnityEngine.Random.Range(0, shellForceRandom), 0, 0), ForceMode.Impulse);
				shellGO.GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(shellSpitTorqueX + UnityEngine.Random.Range(-shellTorqueRandom, shellTorqueRandom), shellSpitTorqueY + UnityEngine.Random.Range(-shellTorqueRandom, shellTorqueRandom), 0), ForceMode.Impulse);
			}
		}
		//NO AVAILABLE MUZZLE
		else { Debug.Log("Gun bullet points not setup properly"); }
	}
}



#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroGun))]
public class SilantroGunEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);
	SilantroGun gun;



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnEnable() { gun = (SilantroGun)target; }



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector(); 
		serializedObject.Update();


		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("System Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponType"), new GUIContent("Type"));
		GUILayout.Space(5f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletType"), new GUIContent("Bullet Type"));


		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Ballistic Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("gunWeight"), new GUIContent("Weight"));
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("barrelLength"), new GUIContent("Barrel Length"));
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("muzzleVelocity"), new GUIContent("Muzzle Velocity"));
		GUILayout.Space(7f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("range"), new GUIContent("Maximum Range"));
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("rateOfFire"), new GUIContent("Rate of Fire"));
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Actual Rate", gun.actualRate.ToString("0.0000"));
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Fire Timer", gun.fireTimer.ToString("0.0000"));

		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Recoil Effect", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Recoil Force", gun.projectileForce.ToString("0.00") + " N");
		GUILayout.Space(3f);
		serializedObject.FindProperty("damperStrength").floatValue = EditorGUILayout.Slider("Damper", serializedObject.FindProperty("damperStrength").floatValue, 0f, 100f);


		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Ammo Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("unlimitedAmmo"), new GUIContent("Infinite Ammo"));
		if (!gun.unlimitedAmmo)
		{
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoCapacity"), new GUIContent("Capacity"));
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Current Ammo", gun.currentAmmo.ToString());
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Drum Weight", gun.drumWeight.ToString() + " kg");
		}
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("ejectShells"), new GUIContent("Release Shells"));
		if (gun.ejectShells)
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("shellEjectPoint"), new GUIContent("Release Point"));
		}



		GUILayout.Space(10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Accuracy Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("accuracy"), new GUIContent("Accuracy"));
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Current Accuracy", gun.currentAccuracy.ToString("0.00"));
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedSettings"), new GUIContent("Advanced Settings"));

		if (gun.advancedSettings)
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("accuracyDrop"), new GUIContent("Drop Per Shot"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("accuracyRecover"), new GUIContent("Recovery Per Shot"));
		}


		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Bullet Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		if (gun.bulletType == SilantroGun.BulletType.Rigidbody)
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ammunition"), new GUIContent("Bullet"));
		}
		else
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"), new GUIContent("Damage"));
		}
		GUILayout.Space(5f);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.BeginVertical();
		SerializedProperty muzs = this.serializedObject.FindProperty("muzzles");
		GUIContent barrelLabel = new GUIContent("Barrel Count");
		EditorGUILayout.PropertyField(muzs.FindPropertyRelative("Array.size"), barrelLabel);
		GUILayout.Space(5f);
		for (int i = 0; i < muzs.arraySize; i++)
		{
			GUIContent label = new GUIContent("Barrel " + (i + 1).ToString());
			EditorGUILayout.PropertyField(muzs.GetArrayElementAtIndex(i), label);
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();


		GUILayout.Space(10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Revolver", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("barrel"), new GUIContent("Revolver"));
		GUILayout.Space(5f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationAxis"), new GUIContent("Rotation Axis"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationDirection"), new GUIContent("Rotation Direction"));

		if (gun.barrel != null)
		{
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Barrel RPM", gun.currentRPM.ToString("0.0") + " RPM");
		}



		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Effects Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("muzzleFlash"), new GUIContent("Muzzle Flash"));
		if (gun.ejectShells)
		{
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletCase"), new GUIContent("Bullet Case"));
		}
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox("Impact Effects", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("groundHit"), new GUIContent("Ground Hit"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("metalHit"), new GUIContent("Metal Hit"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("woodHit"), new GUIContent("Wood Hit"));



		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("fireLoopSound"), new GUIContent("Fire Loop Sound"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("fireEndSound"), new GUIContent("Fire End Sound"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("soundVolume"), new GUIContent("Sound Volume"));
		GUILayout.Space(3f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("soundRange"), new GUIContent("Sound Range"));

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
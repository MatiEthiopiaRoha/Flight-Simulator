using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oyedoyin;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif





[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class SilantroMunition : MonoBehaviour
{

	// ---------------------------------------- Selectibles
	public enum MunitionType { Missile, Rocket, Bullet, Bomb }
	public MunitionType munitionType = MunitionType.Rocket;
	public enum RocketType { Guided, Unguided }
	public RocketType rocketType = RocketType.Unguided;
	public enum FuzeType
	{
		MK352,//Time
		M423,//Proximity
		MK193Mod0,//Impact
	}
	public FuzeType fuzeType = FuzeType.M423;
	public enum TriggerMechanism { Proximity, ImpactForce }
	public TriggerMechanism triggerMechanism = TriggerMechanism.ImpactForce;
	public enum MissileType { ASM, AAM, SAM }
	public MissileType missileType = MissileType.ASM;
	public enum DetonationType { Proximity, Impact, Timer }
	public DetonationType detonationType = DetonationType.Impact;
	public enum FireMode { ForwardFiring, PointFiring }
	public FireMode fireMode = FireMode.ForwardFiring;
	public enum AmmunitionType { AP, HEI, FMJ, Tracer }
	public AmmunitionType ammunitionType = AmmunitionType.Tracer;
	public enum AmmunitionForm
	{
		SecantOgive,//0.171
		TangentOgive,//0.165
		RoundNose,//0.235
		FlatNose,//0.316
		Spitzer//0.168
	}
	public AmmunitionForm ammunitionForm = AmmunitionForm.RoundNose;
	public enum BulletFuzeType { M1032, ME091 }
	public BulletFuzeType bulletfuseType = BulletFuzeType.M1032;
	public enum DragMode { Free, Clamped }
	public DragMode dragMode = DragMode.Clamped;
	public enum SurfaceFinish { SmoothPaint, PolishedMetal, ProductionSheetMetal, MoldedComposite, PaintedAluminium }
	public SurfaceFinish surfaceFinish = SurfaceFinish.PaintedAluminium;




	// ---------------------------------------- Variables
	public string Identifier;
	public float mass;
	public float caseLength;
	public float overallLength;
	public float diameter;
	float area;

	public float munitionDiameter = 1f;
	public float munitionLength = 1f;
	public float munitionWeight = 5f;
	public float maximumRange = 1000f;
	public float distanceTraveled;
	public float activeTime;

	public float timer = 10f;
	public float selfDestructTimer;
	public float triggerTimer;
	public float proximity = 100f;//Distance to target
	bool lostTarget;
	public bool armed;
	bool exploded;

	public float detonationDistance = 100f;
	public float speedThreshhold = 10;

	public float CDCoefficient;
	public float surfaceArea;
	public float percentageSkinning = 70f;
	public float dragForce;
	public float fillingWeight = 10f;
	public bool falling;
	public float speed;
	public float distanceToTarget;
	public float fallTime;

	public float ballisticVelocity;
	float currentVelocity;
	public float currentEnergy;
	public float drag;
	public float skinningRatio;
	public float dragCoefficient;
	public float machSpeed;
	public float airDensity;
	private float viscocity;
	public float altitude;
	public float destroyTime;
	float bullettimer;

	public float damage;
	float damageMulitplier;
	float damageFactor;//f-a
	float damageCompiler;//a
	Vector3 dropVelocity;
	Quaternion collisionRotation;
	Vector3 contactPosition;

	public float skinDragCoefficient;
	public float k, totalDrag, RE;
	public float maximumMachSpeed = 1f;
	public float baseDragCoefficient = 0.05f;
	AnimationCurve frictionDragCurve;
	RaycastHit hit;

	// ---------------------------------------- Connections
	public SilantroController controller;
	public Rigidbody connectedAircraft;
	public Vector3 ejectionPoint;
	public Transform target;
	public Rigidbody munition;
	public SilantroRocketMotor motorEngine;
	public SilantroComputer computer;
	public Collider ammunitionCasing;
	public SilantroPylon connectedPylon;


	// ---------------------------------------- Effects
	public GameObject muzzleFlash;
	public GameObject groundHit;
	public GameObject metalHit;
	public GameObject woodHit;//ADD MORE
	public GameObject explosionPrefab;
	private float soundSpeed;
	private float ambientTemperature;
	private float ambientPressure;





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	/// <summary>
	/// Fires the bullet.
	/// </summary>
	/// <param name="muzzleVelocity"> muzzle/exit velocity of the gun.</param>
	/// <param name="parentVelocity">velocity vector of the parent aircraft.</param>
	public void FireBullet(float muzzleVelocity, Vector3 parentVelocity)
	{
		//DETERMINE INITIAL SPEED
		float startingSpeed;
		if (muzzleVelocity > ballisticVelocity) { startingSpeed = muzzleVelocity; }
		else { startingSpeed = ballisticVelocity; }

		//ADD BASE SPEED
		Vector3 ejectVelocity = transform.forward * startingSpeed;
		Vector3 resultantVelocity = ejectVelocity + parentVelocity;
		//RELEASE BULLET
		munition.velocity = resultantVelocity;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	/// <summary>
	/// Releases the bomb or rocket.
	/// </summary>
	public void ReleaseMunition()
	{
		//GET VELOCITY FROM PARENT
		if (connectedAircraft != null)
		{
			dropVelocity = connectedAircraft.velocity;
		}
		//LAUNCH ROCKET
		if (munitionType == MunitionType.Rocket)
		{
			munition.transform.parent = null;
			munition.isKinematic = false;
			munition.velocity = dropVelocity;
			motorEngine.FireRocket();
			StartCoroutine(TimeStep(0.3f));
		}
		//DROP BOMB
		if (munitionType == MunitionType.Bomb)
		{
			munition.transform.parent = null;
			munition.isKinematic = false;
			munition.velocity = dropVelocity;
			StartCoroutine(TimeStep(1f));
			falling = true;
		}
	}







	bool initialized;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SMART/GUIDED MUNITIONS
	/// <summary>
	/// Fires guided munition.
	/// </summary>
	/// <param name="markedTarget">locked target from the radar.</param>
	/// <param name="ID">tracking ID for the locked target</param>
	/// <param name="mode">fire mode for the missiles; 1: Drop, 2: Tube, 3: Trapeze Launch</param>
	public void FireMunition(Transform markedTarget, string ID, int mode)
	{
		//GET VELOCITY FROM PARENT
		if (connectedAircraft != null)
		{
			dropVelocity = connectedAircraft.velocity;
		}
		//LAUNCH ROCKET
		if (munitionType == MunitionType.Rocket)
		{
			if (computer != null && rocketType == RocketType.Guided)
			{
				//FIRE
				motorEngine.FireRocket();
				munition.transform.parent = null;
				munition.isKinematic = false;
				munition.velocity = dropVelocity;
				StartCoroutine(TimeStep(0.3f));
				//SET TARGET DATA
				computer.Target = markedTarget;
				computer.targetID = ID;
				target = markedTarget;
				//ACTIVATE SEEKER
				computer.seeking = true;
				computer.active = true;
			}
		}
		//LAUNCH MISSILE
		if (munitionType == MunitionType.Missile)
		{
			//1. DROP MISSILE
			if (mode == 1)
			{
				munition.transform.parent = null;
				munition.isKinematic = false;
				munition.velocity = dropVelocity;
				//FIRE
				StartCoroutine(WaitForDrop(markedTarget, ID));
			}
			//2. TUBE LAUNCH
			if (mode == 2)
			{
				//FIRE
				motorEngine.FireRocket();
				munition.transform.parent = null;
				munition.isKinematic = false;
				munition.velocity = dropVelocity;
				StartCoroutine(TimeStep(0.8f));
				//SET TARGET DATA
				computer.Target = markedTarget;
				computer.targetID = ID;
				target = markedTarget;
				//DISABLE GRAVITY
				munition.useGravity = false;
				//ACTIVATE SEEKER
				computer.seeking = true;
				computer.active = true;
			}


			//3. TRAPEZE LAUNCH RIGHT
			if (mode == 3)
			{
				munition.transform.parent = null;
				munition.isKinematic = false;
				munition.velocity = dropVelocity;
				//PUSH OUT
				float pushForce = munition.mass * 500f;
				Vector3 force = munition.transform.right * pushForce;
				munition.AddForce(force);
				//FIRE
				StartCoroutine(WaitForDrop(markedTarget, ID));
			}

			//4. TRAPEZE LAUNCH LEFT
			if (mode == 4)
			{
				munition.transform.parent = null;
				munition.isKinematic = false;
				munition.velocity = dropVelocity;
				//PUSH OUT
				float pushForce = munition.mass * 500f;
				Vector3 force = munition.transform.right * -pushForce;
				munition.AddForce(force);
				//FIRE
				StartCoroutine(WaitForDrop(markedTarget, ID));
			}

			//5. TRAPEZE LAUNCH MIDDLE
			if (mode == 5)
			{
				munition.transform.parent = null;
				munition.isKinematic = false;
				munition.velocity = dropVelocity;
				//PUSH OUT
				float pushForce = munition.mass * 800f;
				Vector3 force = munition.transform.up * -pushForce;
				munition.AddForce(force);
				//FIRE
				StartCoroutine(WaitForDrop(markedTarget, ID));
			}
			//REMOVE PYLON
			if (connectedPylon != null && connectedPylon.pylonPosition == SilantroPylon.PylonPosition.External)
			{
				Destroy(connectedPylon.gameObject);
			}
		}
	}








	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ACTIVATE AFTER DROP
	IEnumerator WaitForDrop(Transform markedTarget, string ID)
	{
		yield return new WaitForSeconds(1f);
		////FIRE
		motorEngine.FireRocket();
		StartCoroutine(TimeStep(0.8f));

		//SET TARGET DATA
		computer.Target = markedTarget;
		computer.targetID = ID;

		target = markedTarget;
		//DISABLE GRAVITY
		munition.useGravity = false;

		//ACTIVATE SEEKER
		computer.seeking = true;
		computer.active = true;
	}


	//CLEAR AIRCRAFT BEFORE ARMING
	IEnumerator TimeStep(float time)
	{
		yield return new WaitForSeconds(time);
		armed = true;
	}










	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SETUP WEAPON
	public void InitializeMunition()
	{
		//GET COMPONENTS
		munition = GetComponent<Rigidbody>();

		//SET FINISH FACTOR
		if (surfaceFinish == SurfaceFinish.MoldedComposite) { k = 0.17f; }
		if (surfaceFinish == SurfaceFinish.PaintedAluminium) { k = 3.33f; }
		if (surfaceFinish == SurfaceFinish.PolishedMetal) { k = 0.50f; }
		if (surfaceFinish == SurfaceFinish.ProductionSheetMetal) { k = 1.33f; }
		if (surfaceFinish == SurfaceFinish.SmoothPaint) { k = 2.08f; }

		frictionDragCurve = new AnimationCurve();
		frictionDragCurve.AddKey(new Keyframe(1000000000, 1.5f));
		frictionDragCurve.AddKey(new Keyframe(100000000, 2.0f));
		frictionDragCurve.AddKey(new Keyframe(10000000, 2.85f));
		frictionDragCurve.AddKey(new Keyframe(1000000, 4.1f));
		frictionDragCurve.AddKey(new Keyframe(100000, 7.0f));


		//SETUP MISSILES-ROCKETS-BOMBS
		if (munitionType != MunitionType.Bullet)
		{
			computer = GetComponentInChildren<SilantroComputer>();
			if (computer == null)
			{
				Debug.Log("No computer is connected to munition " + transform.name);
			}
			else
			{
				if (munition != null)
				{
					computer.munition = this.GetComponent<SilantroMunition>();
				}
			}
			//SET ROCKET MOTOR PROPERTIES
			if (motorEngine != null)
			{
				motorEngine.weapon = munition;
				motorEngine.InitializeRocket();
			}
		}

		//SET FACTORS
		if (munition != null)
		{
			if (munitionType != MunitionType.Bullet)
			{
				munition.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
				munition.mass = munitionWeight;
				munition.isKinematic = true;
			}
			else { munition.mass = ((mass * 0.0648f) / 1000f); }

		}
		else
		{
			Debug.Log("Rigidbody for munition is missing " + transform.name);
		}
		armed = false;
		//SEND DATA
		if (munitionType == MunitionType.Missile)
		{
			computer.munition = this.GetComponent<SilantroMunition>();
			computer.InitializeComputer();
		}



		if (munitionType == MunitionType.Bomb)
		{
			//CALCULATE AREA
			float radius = munitionDiameter / 2;
			float a1 = 3.142f * radius * radius;
			float error = UnityEngine.Random.Range((percentageSkinning - 4.5f), (percentageSkinning + 5f));
			surfaceArea = (a1) * (error / 100f);
		}
		else { surfaceArea = 2f * Mathf.PI * (munitionDiameter / 2f) * munitionLength; }




		if (munitionType == MunitionType.Bullet)
		{
			//SET AERODYNAMIC PROPERTIES
			if (ammunitionForm == AmmunitionForm.FlatNose)
			{
				skinningRatio = 0.99f;
				dragCoefficient = 0.316f;
			}
			else if (ammunitionForm == AmmunitionForm.SecantOgive)
			{
				skinningRatio = 0.913f;
				dragCoefficient = 0.171f;
			}
			else if (ammunitionForm == AmmunitionForm.RoundNose)
			{
				skinningRatio = 0.95f;
				dragCoefficient = 0.235f;
			}
			else if (ammunitionForm == AmmunitionForm.TangentOgive)
			{
				skinningRatio = 0.914f;
				dragCoefficient = 0.165f;
			}
			else if (ammunitionForm == AmmunitionForm.Spitzer)
			{
				skinningRatio = 0.921f;
				dragCoefficient = 0.168f;
			}
			//
			ammunitionCasing = GetComponent<Collider>();
			if (munition == null)
			{
				Debug.Log("Bullet " + transform.name + " rigidbody has not been assigned");
			}
			else
			{
				if (ammunitionCasing == null)
				{
					Debug.Log("Ammunition cannot work without a collider");
				}
				else
				{
					float radius = diameter / 2000f;
					area = Mathf.PI * radius * radius;
				}
			}
			//
			float a = Random.Range(78, 98);
			float f = Random.Range(27, 43);
			damageCompiler = a;
			damageFactor = f - a;
		}
		initialized = true;
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float EstimateRe(float inputSpeed)
	{
		float superRe = 0f;
		float Re1 = (airDensity * inputSpeed * munitionLength) / viscocity; float Re2;
		if (machSpeed < 0.9f) { Re2 = 38.21f * Mathf.Pow(((munitionLength * 3.28f) / (k / 100000)), 1.053f); }
		else { Re2 = 44.62f * Mathf.Pow(((munitionLength * 3.28f) / (k / 100000)), 1.053f) * Mathf.Pow(machSpeed, 1.16f); }
		superRe = Mathf.Min(Re1, Re2); RE = superRe;
		return superRe;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float EstimateSkinDragCoefficient(float velocity)
	{
		float Recr = EstimateRe(velocity);
		float baseCf = frictionDragCurve.Evaluate(Recr) / 1000f;

		//WRAPPING CORRECTION
		float Cfa = baseCf * (0.0025f * (munitionLength / munitionDiameter) * Mathf.Pow(Recr, -0.2f));
		//SUPERVELOCITY CORRECTION
		float Cfb = baseCf * Mathf.Pow((munitionDiameter / munitionLength), 1.5f);
		//PRESSURE CORRECTION
		float Cfc = baseCf * 7 * Mathf.Pow((munitionDiameter / munitionLength), 3f);
		float actualCf = 1.03f * (baseCf + Cfa + Cfb + Cfc);
		return actualCf;
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//DRAW MARKERS
#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		//SEND DATA TO ENGINE
		if (motorEngine != null && munitionType != MunitionType.Bullet)
		{
			motorEngine.boosterDiameter = munitionDiameter;
			motorEngine.overallLength = munitionLength;
		}
		Identifier = transform.name;

		frictionDragCurve = new AnimationCurve();
		frictionDragCurve.AddKey(new Keyframe(1000000000, 1.5f));
		frictionDragCurve.AddKey(new Keyframe(100000000, 2.0f));
		frictionDragCurve.AddKey(new Keyframe(10000000, 2.85f));
		frictionDragCurve.AddKey(new Keyframe(1000000, 4.1f));
		frictionDragCurve.AddKey(new Keyframe(100000, 7.0f));

		//DRAW IDENTIFIER
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(transform.position, 0.1f);
		Gizmos.color = Color.yellow;
		if (connectedAircraft != null)
		{
			Gizmos.DrawLine(this.transform.position, (connectedAircraft.transform.up * 2f + this.transform.position));
		}
		else
		{
			Gizmos.DrawLine(this.transform.position, (this.transform.up * 2f + this.transform.position));
		}
	}
#endif






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//MUNITION COLLISION
	void OnCollisionEnter(Collision col)
	{
		if (controller != null && col.gameObject != controller.gameObject && initialized)
		{
			//GET CONTACT POINT
			ContactPoint contact = col.contacts[0];
			collisionRotation = Quaternion.FromToRotation(Vector3.up, contact.normal);

			//FIRE TYPE
			if (fireMode == FireMode.PointFiring) { contactPosition = contact.point; }
			if (fireMode == FireMode.ForwardFiring) { if (Physics.Raycast(transform.position, munition.velocity, out hit)) { contactPosition = hit.point; } }

			//1. ---------------------------------------------------------------------------ROCKET
			if (munitionType == MunitionType.Rocket)
			{
				//TRIGGER WITH IMPACT
				if (armed && fuzeType == FuzeType.MK193Mod0)
				{
					Explode("Rocket Collision--Fuze MK193", contactPosition);
				}
				//DESTROY IF IMPACT IS STRONG ENOUGH
				if (armed && munition.velocity.magnitude > 5)
				{
					StartCoroutine(WaitForMomentumShead("Rocket Collision Base", contactPosition));
				}
			}


			//2. ---------------------------------------------------------------------------BOMB
			if (munitionType == MunitionType.Bomb)
			{
				//TRIGGER WITH IMPACT
				if (armed && triggerMechanism == TriggerMechanism.ImpactForce && speed > speedThreshhold)
				{
					StartCoroutine(WaitForMomentumShead("Bomb Collision Fuze", contactPosition));
				}
				//DESTROY IF IMPACT IS STRONG ENOUGH
				if (armed && speed > 5)
				{
					StartCoroutine(WaitForMomentumShead("Bomb Speed Trigger", contactPosition));
				}
			}


			//3.---------------------------------------------------------------------------- MISSILE
			if (munitionType == MunitionType.Missile)
			{
				//TRIGGER WITH IMPACT
				if (armed && detonationType == DetonationType.Impact && col.relativeVelocity.magnitude > speedThreshhold)
				{
					StartCoroutine(WaitForMomentumShead("Missile Impact Fuze", contactPosition));
				}
				//DESTROY IF IMPACT IS STRONG ENOUGH
				if (armed && col.relativeVelocity.magnitude > 5)
				{
					StartCoroutine(WaitForMomentumShead("Missile Base Fuze", contactPosition));
				}
			}



			//4. -------------------------------------------------------------------------------------------BULLET
			if (munitionType == MunitionType.Bullet)
			{
				if (ammunitionType == AmmunitionType.HEI)
				{
					Explode("Bullet Collision--HEI", contactPosition);
				}
				if (ammunitionType == AmmunitionType.AP)
				{
					damageMulitplier = 5f;
				}
				if (ammunitionType == AmmunitionType.FMJ)
				{
					damageMulitplier = 1.56f;
				}
				if (ammunitionType == AmmunitionType.Tracer)
				{
					damageMulitplier = 1.65f;
				}


				//------------------------------DISTANCE FALLOFF
				float distance = Vector3.Distance(this.transform.position, ejectionPoint);
				float mix = (((distance - 10f) * damageFactor) / 1990) + damageCompiler;
				float actualDamage = damage * damageMulitplier * (mix / 100f);
				//APPLY
				if (actualDamage > 0)
				{
					col.collider.gameObject.SendMessage("SilantroDamage", actualDamage, SendMessageOptions.DontRequireReceiver);
				}
				//EFFECT
				if (col.collider.tag == "Ground" && groundHit != null)
				{
					Instantiate(groundHit, col.contacts[0].point, Quaternion.FromToRotation(Vector3.up, col.contacts[0].normal));
				}
				if (col.collider.tag == "Wood" && woodHit != null)
				{
					Instantiate(woodHit, col.contacts[0].point, Quaternion.FromToRotation(Vector3.up, col.contacts[0].normal));
				}
				if (col.collider.tag == "Metal" && metalHit != null)
				{
					Instantiate(metalHit, col.contacts[0].point, Quaternion.FromToRotation(Vector3.up, col.contacts[0].normal));
				}


				Destroy(gameObject);
			}
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SIMULATE ACTIVATION LATENCY
	IEnumerator WaitForMomentumShead(string actionLabel, Vector3 collisionPoint)
	{
		yield return new WaitForSeconds(0.02f);
		Explode(actionLabel, collisionPoint);
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//WARHEAD ACTIVATION
	public void Explode(string actionLabel, Vector3 collisionPosition)
	{
		if (explosionPrefab != null && !exploded)
		{
			GameObject explosion = Instantiate(explosionPrefab, collisionPosition, collisionRotation);
			explosion.SetActive(true);
			explosion.GetComponentInChildren<AudioSource>().Play();
			exploded = true;
		}

		Destroy(gameObject);
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//BULLET DATA
	void CalculateData()
	{
		if (initialized)
		{
			float currentAltitude = transform.position.y;
			float kiloMeter = MathBase.ConvertDistance(currentAltitude, "FT");
			float a = 0.0000004f * kiloMeter * kiloMeter; float b = (0.0351f * kiloMeter);
			ambientPressure = (a - b + 1009.6f) / 10f;
			float a1 = 0.000000003f * kiloMeter * kiloMeter; float a2 = 0.0021f * kiloMeter;
			ambientTemperature = a1 - a2 + 15.443f;
			float kelvinTemperatrue = ambientTemperature + 273.15f;
			airDensity = (ambientPressure * 1000f) / (287.05f * kelvinTemperatrue);
			viscocity = 1.458f / (1000000) * Mathf.Pow(kelvinTemperatrue, 1.5f) * (1 / (kelvinTemperatrue + 110.4f));
			soundSpeed = Mathf.Pow((1.2f * 287f * (273.15f + ambientTemperature)), 0.5f);
			currentVelocity = munition.velocity.magnitude;
			machSpeed = currentVelocity / soundSpeed;
			airDensity = (ambientPressure * 1000f) / (287.05f * kelvinTemperatrue);

			//BULLET DRAG
			drag = 0.5f * airDensity * dragCoefficient * area * currentVelocity * currentVelocity;
			//BULLET ENERGY
			currentEnergy = 0.5f * (mass / 1000) * currentVelocity * currentVelocity;
		}
	}





	private void LateUpdate()
	{
		if (munition != null && armed && munitionType == MunitionType.Bomb)
		{
			munition.transform.forward = Vector3.Slerp(munition.transform.forward, munition.velocity.normalized, Time.deltaTime);
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{

		if (initialized)
		{

			
			if (munitionType == MunitionType.Missile || munitionType == MunitionType.Rocket)
			{
				//CALCULATE DRAG COEFFICIENT
				if (dragMode == DragMode.Clamped)
				{
					float trueSpeed = soundSpeed * maximumMachSpeed;
					float dynamicForce = 0.5f * airDensity * trueSpeed * trueSpeed * surfaceArea;
					dragCoefficient = motorEngine.Thrust / dynamicForce;
					if (motorEngine.Thrust < 1 && motorEngine.active) { dragMode = DragMode.Free; baseDragCoefficient = 0.005f; }
				}
				if (dragMode == DragMode.Free)
				{
					skinDragCoefficient = EstimateSkinDragCoefficient(speed);
					dragCoefficient = skinDragCoefficient + baseDragCoefficient;
				}

				//Drag
				if (float.IsNaN(dragCoefficient) || float.IsInfinity(dragCoefficient)) { dragCoefficient = 0.01f; }
				if(machSpeed > maximumMachSpeed) { dragMode = DragMode.Clamped; }
				dragForce = 0.5f * airDensity * dragCoefficient * speed * speed * surfaceArea;

				if (dragForce > 0)
				{
					Vector3 Force = transform.forward * -dragForce;
					if (!float.IsNaN(drag) && !float.IsInfinity(drag)) { munition.AddForce(Force, ForceMode.Force); }
				}
			}

			//----------------------------------------------------------------------------BOMBS
			if (munitionType == MunitionType.Bomb)
			{
				Vector3 force = -munition.velocity.normalized * dragForce;
				munition.AddForce(force, ForceMode.Force);
			}

			//-----------------------------------------------------------------------------BULLET
			if (drag > 0 && munitionType == MunitionType.Bullet)
			{
				Vector3 Force = transform.forward * -drag;
				if (!float.IsNaN(drag) && !float.IsInfinity(drag)) { munition.AddForce(Force, ForceMode.Force); }
			}




			//------------------------------------------------------------------------------------------------------------------------GENERAL
			if (armed)
			{
				speed = munition.velocity.magnitude;
				activeTime += Time.deltaTime;
				distanceTraveled += speed * Time.deltaTime;
				if (computer != null) { if (computer.Target != null) { distanceToTarget = Vector3.Distance(transform.position, computer.Target.position); } }

				//SEND DATA
				CalculateData();
			}


			//------------------------------------------------------------------------------------------------------------------------BULLET
			if (munitionType == MunitionType.Bullet)
			{
				bullettimer += Time.deltaTime;
				if (bullettimer > destroyTime && bulletfuseType == BulletFuzeType.M1032)
				{
					Destroy(gameObject);
				}
			}



			//OTHERS
			if (armed)
			{
				if (explosionPrefab != null && computer != null && computer.Target != null)
				{
					SilantroExplosion explosion = explosionPrefab.GetComponent<SilantroExplosion>();
					if (explosion != null)
					{
						if (distanceToTarget < (0.6f * explosion.explosionRadius)) { Explode("Missile Proximity Fuze", transform.position); }
					}
				}



				//1.------------------------------------------------------------------------------------------------------------------ROCKET
				if (munitionType == MunitionType.Rocket)
				{
					//TIMER FUZE
					if (fuzeType == FuzeType.MK352)
					{
						triggerTimer += Time.deltaTime;
						if (triggerTimer > timer)
						{
							Explode("Rocket Timer--Fuze MK352", transform.position);
						}
					}
					//PROXIMITY FUZE
					if (fuzeType == FuzeType.M423)
					{
						//ACTIVATE IF TARGET IS WITHIN RANGE
						if (computer.Target != null)
						{
							if (distanceToTarget < proximity)
							{
								Explode("Rocket Proximity--Fuze M423", transform.position);
							}
						}
					}
					//RESET TIMER
					if (target)
					{
						selfDestructTimer = 0f;
					}
					////DESTROY IF TARGET IS NULL
					if (rocketType == RocketType.Guided && computer.seeking && computer.Target == null)
					{
						{   //DESTROY AFTER 5 Seconds IF TARGET IS NULL
							selfDestructTimer += Time.deltaTime;
							if (selfDestructTimer > 5)
							{
								Explode("Rocket Self Destruct", transform.position);
							}
						}
					}
				}




				//2.----------------------------------------------------------------------------------------------------------------------------------BOMB
				if (munitionType == MunitionType.Bomb)
				{
					if (speed > 0 && falling)
					{
						fallTime += Time.deltaTime;
					}
					//CALCULATE DRAG
					if (falling)
					{
						//CALCULATE REYNOLDS NUMBER
						float viscocity = (0.00000009f * ambientTemperature) + 0.00001f;
						float reynolds = (airDensity * speed * munitionLength) / viscocity;
						//CALCULATE CD
						if (reynolds < 1000000) { CDCoefficient = (0.1f * Mathf.Log10(reynolds)) - 0.4f; }
						else { CDCoefficient = 0.19f - (80000 / reynolds); }
						//DRAG
						dragForce = (0.5f * airDensity * CDCoefficient * surfaceArea * speed * speed);
					}
				}







				//3.------------------------------------------------------------------------------------------------------------------------------- MISSILE
				if (munitionType == MunitionType.Missile)
				{

					//-----------------------------------------------TIMER FUZE
					if (detonationType == DetonationType.Timer)
					{
						triggerTimer += Time.deltaTime;
						if (triggerTimer > timer)
						{
							Explode("Missile Timer Fuze", transform.position);
						}
					}


					//-----------------------------------------------PROXIMITY FUZE
					if (detonationType == DetonationType.Proximity)
					{
						//ACTIVATE IF TARGET IS WITHIN RANGE
						if (computer.Target != null)
						{
							if (distanceToTarget < proximity)
							{
								Explode("Missile Proximity Fuze", transform.position);
							}
						}
					}


					//----------------------------------------------RESET TIMER
					if (target) { selfDestructTimer = 0f; }


					////--------------------------------------------DESTROY IF TARGET IS NULL
					if (computer.seeking && computer.Target == null)
					{
						{
							//DESTROY AFTER 5 Seconds IF TARGET IS NULL
							selfDestructTimer += Time.deltaTime;
							if (selfDestructTimer > 5) { Explode("Missile Self Destruct Fuze", transform.position); }
						}
					}
				}


				//---------------------------------------------------------------------------------------------------------DESTROY IF OUT OF RANGE

			}
		}
	}
}






#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroMunition))]
public class SilantroMunitionEditor : Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1, 0.4f, 0);
	SilantroMunition munition;

	//--------------------------------------------------------------------------------------------------------
	private void OnEnable() { munition = (SilantroMunition)target; }


	//--------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector(); 
		serializedObject.Update();

		EditorGUILayout.LabelField("Identifier", munition.Identifier);
		GUILayout.Space(3f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox("Munition Type", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionType"), new GUIContent(" "));


		if (munition.munitionType == SilantroMunition.MunitionType.Rocket || munition.munitionType == SilantroMunition.MunitionType.Missile)
		{
			GUILayout.Space(8f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Dynamic Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("surfaceFinish"), new GUIContent("Surface Finish"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("dragMode"), new GUIContent("Drag Estimation"));
			GUILayout.Space(5f);
			if (munition.dragMode == SilantroMunition.DragMode.Clamped)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumMachSpeed"), new GUIContent("Maximum Mach Speed"));
			}
			if (munition.dragMode == SilantroMunition.DragMode.Free)
			{
				GUI.color = Color.white;
				EditorGUILayout.HelpBox("Base Drag Coefficient", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("baseDragCoefficient"), new GUIContent(" "));
				GUILayout.Space(3f);
				EditorGUILayout.LabelField("Total Cd", munition.dragCoefficient.ToString("0.000"));
			}
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Current Speed", munition.machSpeed.ToString("0.00") + " Mach");
		}

		GUILayout.Space(10f);

		//1. -------------------------------------------- Rocket
		if (munition.munitionType == SilantroMunition.MunitionType.Rocket)
		{
			GUILayout.Space(2f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Rocket Type", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rocketType"), new GUIContent("Mode"));
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Warhead Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("fireMode"), new GUIContent("Fire Mode"));
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionPrefab"), new GUIContent("Explosion Prefab"));


			if (munition.explosionPrefab != null)
			{
				SilantroExplosion explosive = munition.explosionPrefab.GetComponent<SilantroExplosion>();
				if (explosive != null)
				{
					GUI.color = Color.white;
					EditorGUILayout.HelpBox("Performance", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space(2f);
					GUILayout.Space(3f);
					EditorGUILayout.LabelField("Explosive Force", explosive.explosionForce.ToString("0.0") + " N");
					GUILayout.Space(1f);
					EditorGUILayout.LabelField("Explosive Radius", explosive.explosionRadius.ToString("0") + " m");
				}
			}

			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Detonation System", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("fuzeType"), new GUIContent("Fuze Type"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Armed", munition.armed.ToString());
			GUILayout.Space(3f);
			//IMPACT
			if (munition.fuzeType == SilantroMunition.FuzeType.MK193Mod0)
			{
				EditorGUILayout.LabelField("Detonation Mechanism", "Nose Impact");
				GUILayout.Space(2f);
			}
			//TIME
			else if (munition.fuzeType == SilantroMunition.FuzeType.MK352)
			{
				EditorGUILayout.LabelField("Detonation Mechanism", "Mechanical Timer");
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("timer"), new GUIContent("Trigger Timer"));
			}
			else if (munition.fuzeType == SilantroMunition.FuzeType.M423)
			{
				EditorGUILayout.LabelField("Detonation Mechanism", "Proximity");
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("proximity"), new GUIContent("Trigger Distance"));
				if (munition.target != null)
				{
					GUILayout.Space(2f);
					EditorGUILayout.LabelField("Target", munition.target.name);
				}
			}
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRange"), new GUIContent("Maximum Range"));
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Rocket Dimensions", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionDiameter"), new GUIContent("Diameter"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionLength"), new GUIContent("Length"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionWeight"), new GUIContent("Weight"));
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Propulsion", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("motorEngine"), new GUIContent("Rocket Motor"));
			GUILayout.Space(3f);
		}

		//2. -------------------------------------------- Bullet
		if (munition.munitionType == SilantroMunition.MunitionType.Bullet)
		{
			GUILayout.Space(3f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Bullet Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ammunitionType"), new GUIContent("Ammunition Type"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ammunitionForm"), new GUIContent("Ammunition Form"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletfuseType"), new GUIContent("Fuze Type"));
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("System Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("mass"), new GUIContent("Mass"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("caseLength"), new GUIContent("Case Length"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("overallLength"), new GUIContent("Overall Length"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("diameter"), new GUIContent("Diameter"));
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Performance Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ballisticVelocity"), new GUIContent("Ballistic Velocity"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"), new GUIContent("Damage"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("destroyTime"), new GUIContent("Destroy Time"));
			if (munition.ammunitionType == SilantroMunition.AmmunitionType.HEI)
			{
				GUILayout.Space(10f);
				GUI.color = silantroColor;
				EditorGUILayout.HelpBox("Explosive Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(3f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionPrefab"), new GUIContent("Explosion Prefab"));
				if (munition.explosionPrefab != null)
				{
					SilantroExplosion explosive = munition.explosionPrefab.GetComponent<SilantroExplosion>();
					if (explosive != null)
					{
						GUI.color = Color.white;
						EditorGUILayout.HelpBox("Performance", MessageType.None);
						GUI.color = backgroundColor;
						GUILayout.Space(3f);
						EditorGUILayout.LabelField("Explosive Force", explosive.explosionForce.ToString("0.0") + " N");
						GUILayout.Space(1f);
						EditorGUILayout.LabelField("Explosive Radius", explosive.explosionRadius.ToString("0") + " m");
					}
				}
			}
		}

		//3. -------------------------------------------- Missile
		if (munition.munitionType == SilantroMunition.MunitionType.Missile)
		{
			GUILayout.Space(2f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Missile Configuration", MessageType.None);
			GUI.color = backgroundColor;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("missileType"), new GUIContent("Mode"));
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Warhead Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("fireMode"), new GUIContent("Fire Mode"));
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionPrefab"), new GUIContent("Explosion Prefab"));
			if (munition.explosionPrefab != null)
			{
				SilantroExplosion explosive = munition.explosionPrefab.GetComponent<SilantroExplosion>();
				if (explosive != null)
				{
					GUI.color = Color.white;
					EditorGUILayout.HelpBox("Performance", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space(2f);
					GUILayout.Space(3f);
					EditorGUILayout.LabelField("Explosive Force", explosive.explosionForce.ToString("0.0") + " N");
					GUILayout.Space(1f);
					EditorGUILayout.LabelField("Explosive Radius", explosive.explosionRadius.ToString("0") + " m");
				}
			}

			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Detonation System", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Armed State", munition.armed.ToString());
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Current Speed", munition.speed.ToString() + " m/s");
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("detonationType"), new GUIContent("Detonation Type"));
			if (munition.detonationType == SilantroMunition.DetonationType.Proximity)
			{
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("proximity"), new GUIContent("Trigger Distance"));
				if (munition.computer != null && munition.computer.Target)
				{
					GUILayout.Space(2f);
					EditorGUILayout.LabelField("Distance To Target", munition.distanceToTarget.ToString("0.00") + " m");
				}
				if (munition.target != null)
				{
					EditorGUILayout.LabelField("Current Target", munition.target.name);
				}
				else
				{
					EditorGUILayout.LabelField("Current Target", "Null");
				}
			}
			if (munition.detonationType == SilantroMunition.DetonationType.Timer)
			{
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("timer"), new GUIContent("Trigger Timer"));
			}
			if (munition.detonationType == SilantroMunition.DetonationType.Impact)
			{
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("speedThreshhold"), new GUIContent("Trigger Speed"));
			}
			GUILayout.Space(5f);
			EditorGUILayout.LabelField("Distance Traveled", munition.distanceTraveled.ToString("0.0") + " m");
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRange"), new GUIContent("Maximum Range"));
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Missile Dimensions", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionDiameter"), new GUIContent("Diameter"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionLength"), new GUIContent("Length"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionWeight"), new GUIContent("Weight"));
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Propulsion", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("motorEngine"), new GUIContent("Rocket Motor"));
			GUILayout.Space(3f);
		}

		//4. -------------------------------------------- Bomb
		if (munition.munitionType == SilantroMunition.MunitionType.Bomb)
		{
			GUILayout.Space(2f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Trigger Mechanism", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerMechanism"), new GUIContent(" "));
			if (munition.triggerMechanism == SilantroMunition.TriggerMechanism.ImpactForce)
			{
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("speedThreshhold"), new GUIContent("Trigger Speed"));
			}
			if (munition.triggerMechanism == SilantroMunition.TriggerMechanism.Proximity)
			{
				GUILayout.Space(2f);
				if (munition.target != null)
				{
					EditorGUILayout.LabelField("Target", munition.target.name);
				}
				GUILayout.Space(2f);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("detonationDistance"), new GUIContent("Trigger Distance"));
			}
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Warhead Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("fillingWeight"), new GUIContent("Filling Weight"));
			GUILayout.Space(5f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionPrefab"), new GUIContent("Explosion Prefab"));

			if (munition.explosionPrefab != null)
			{
				SilantroExplosion explosive = munition.explosionPrefab.GetComponent<SilantroExplosion>();
				if (explosive != null)
				{
					GUI.color = Color.white;
					EditorGUILayout.HelpBox("Performance", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space(2f);
					GUILayout.Space(3f);
					EditorGUILayout.LabelField("Explosive Force", explosive.explosionForce.ToString("0.0") + " N");
					GUILayout.Space(1f);
					EditorGUILayout.LabelField("Explosive Radius", explosive.explosionRadius.ToString("0") + " m");
				}
			}

			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Bomb Dimensions", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionDiameter"), new GUIContent("Diameter"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionLength"), new GUIContent("Length"));
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionWeight"), new GUIContent("Weight"));
			GUILayout.Space(2f);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("percentageSkinning"), new GUIContent("Skinning"));
			GUILayout.Space(10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Bomb Performance", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Drop Speed", munition.speed.ToString("0.0") + " m/s");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Distance To Target", (munition.distanceToTarget / 3.286f).ToString("0.0") + " m");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Fall Time", munition.fallTime.ToString("0.0") + " s");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Drag Force", munition.dragForce.ToString("0.0") + " N");
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
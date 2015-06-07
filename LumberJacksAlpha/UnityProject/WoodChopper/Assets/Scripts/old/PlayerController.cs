using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//[System.Serializable]
//condition for going inside GetAxisDown if used in EvaluateAllAxis
public class AxisInput
{
	public string axisName = "";
	public bool axisDown = false;
}

//[System.Serializable]
//condition for triggering movement
public class AxisValue
{
	public float value = 0f;
	public bool axisPressed = false;
}

public class PlayerController : MonoBehaviour
{
	GameObject bloodParticle;

	bool playerSetDown = false;
	int playerIndex = 0;

	bool actionBlock = false;

	Animator anim;
	bool queueFlag;
	Vector3 queuePosition;

	public LayerMask treeMask;
	public LayerMask boudriesMask;
	public LayerMask playerMask;
	public Transform rayShooter;

	float inputMultiplier = 1.0f;

	bool obstacle;

	float speedMultiplier = 2f;

	float animBoost = 0.0f;
	float lastInput = 0.0f;
	float inputHz = 0.0f;
	float animMultiplier = 1.0f;

	float startTime = 0;
	float timer = 0;
	float timerBoost = 0.0f;
	float timerTimer = 0.0f;

	Vector3 startPositionBackup = Vector3.zero;
	Vector3 startPosition = Vector3.zero;
	Vector3 newPosition = Vector3.zero;
	Vector3 currentPosition = Vector3.zero;

	List<GameObject> Obstacles = new List<GameObject>();

	bool movement = false;

	List<AxisInput> allAxis = new List<AxisInput>();

	//setting to false only when we realese the key - condition for going inside if in GetAxisDown
	void EvaluateAllAxis()
	{
		for(int i = 0; i < allAxis.Count; i++)
		{
			if(Mathf.Abs(Input.GetAxis(allAxis[i].axisName)) <= 0.5f)
			{
				allAxis[i].axisDown = false;
			}
		}
	}

	//checking if axis is already on the list
	int AllAxisContainsAxis(string axisName)
	{
		for(int i = 0; i < allAxis.Count; i++)
		{
			if(allAxis[i].axisName == axisName)
			{
				return i;
			}
		}
		return -1;
	}

	//manage the input of controller's axis
	AxisValue GetAxisDown(string axisName)
	{
		int axisIndex = -1;
		//checking if it's right or left, up or down (axisName comes from Input settings
		float value = (Input.GetAxis(axisName) > 0.5f) ? 1f : ((Input.GetAxis(axisName) < -0.5f) ? -1f : 0f);
		AxisValue av = new AxisValue();
		//if we hold down the key
		if( value > 0.5f || value < -0.5f)
		{
			//checking if axis is already on the list
			axisIndex = AllAxisContainsAxis(axisName);
			//if it is on list
			if(axisIndex >= 0)
			{
				//if inside the class object there is false bool
				if(!allAxis[axisIndex].axisDown)
				{
					//set it to true - it won't come here again
					allAxis[axisIndex].axisDown = true;

					av = new AxisValue();

					//setting the appropriate value for the name of the axis
					av.axisPressed = true;
					av.value = value;

					return av;
				}
			}
			//if not on list
			else
			{
				//set the name and value
				AxisInput axis = new AxisInput();
				axis.axisName = axisName;
				axis.axisDown = true;
				allAxis.Add(axis);

				av = new AxisValue();
				av.axisPressed = true;
				av.value = value;

				return av;
			}
		}

		//only pressed on true will trigger movement - will be set to this only once
		av.axisPressed = false;
		av.value = 0f;

		return av;
	}

	void Start () 
	{
		//GameObject bloodObject = (GameObject)Resources.Load ("Blood");
		//bloodParticle = (GameObject)GameObject.Instantiate (bloodObject, transform.position, Quaternion.identity);

		anim = GetComponent<Animator> ();
	}
	
	void Update ()
	{
		Movement();
		HandleInput();
		EvaluateAllAxis();
	}

	void Movement()
	{
		if(movement)
		{
			//Debug.Log("ILE");
			anim.speed = 1.0f * timerBoost;
			timer += Time.deltaTime/4;
			timerTimer += Time.deltaTime*4;

			timerBoost = Mathf.Lerp(timer, inputMultiplier + 1.0f, timerTimer);

			timer += 0.1f*timerBoost;

			currentPosition = Vector3.Lerp(startPosition, newPosition, timer);

			transform.position = currentPosition;

			if(newPosition == currentPosition)
			{
				if(queueFlag == true)
				{
					startPosition = transform.position;
					newPosition = queuePosition;
					queueFlag = false;
					timer = 0f;
					timerTimer = 0.0f;
				}
				else
				{
					movement = false;
					timer = 0f;
					timerTimer = 0.0f;
				}
			}
		}

	}

	//jest szansa, że animacja kończy się trochę później niż jak postać dociera na miejsce - nie będzie idealnie. Wtedy movement się robi na false i jakby tam anim.speed = 1
	//to animacja która ciągle trwa nagle się robi wolniejsza i będzie jeszcze bardziej niedokładnie. Więc event wywoła tę funkcję i dopiero wtedy ustawia animację
	//oczywiście, jak ten jump jest ustawiony na 1 czyli bardzo długi, to też się zwali, bo już po pierwszym jumpie speed się ustawi na jeden i drugi będzie zwolniony
	void AnimSpeedReset()
	{
		if (movement == false)
		{
			//Debug.Log("INSIDE");
			anim.speed = 1.0f;
		}
	}

	void KickUnblock()
	{
		actionBlock = false;
	}

	void HandleInput()
	{
		GameObject treeHit = HitTree(transform.forward);

		AxisValue horAxis = GetAxisDown("AxisHorDown");
		AxisValue verAxis = GetAxisDown("AxisVerDown");

		animBoost += Time.deltaTime;

		if(Input.GetButtonDown("F") || Input.GetButtonDown("PS3Kick"))
		{
			actionBlock = true;
			anim.SetTrigger("kick");
			//GameObject treeHit = HitTree(transform.forward);
			if(treeHit)
			{
				Tree treeScript = treeHit.GetComponent<Tree>();
				if(treeScript.treeIsDead == false)
				{
					treeHit.SendMessage("Kick");

					if(playerSetDown == false)
					{
						treeHit.SendMessage("PlayerSet", playerIndex);
						playerSetDown = true;
					}
				}
			}
		}

		if(Input.GetButton("Jump")  || Input.GetButton("PS3Cut") && actionBlock == false)
		{
			anim.SetBool("cutB", true);
			//anim.SetTrigger("cut");

			if(treeHit)
			{
				Tree treeScript = treeHit.GetComponent<Tree>();
				//Debug.Log(treeScript.treeIsDead);
				if(treeScript.anim && treeScript.cutParticle && treeScript.treeIsDead == false)
				{
					treeScript.anim.SetBool("TreeCut", true);
					treeHit.SendMessage("Cut");
					treeScript.cutParticle.GetComponent<ParticleSystem>().Play();

					if(playerSetDown == false)
					{
						treeHit.SendMessage("PlayerSet", playerIndex);
						playerSetDown = true;
					}
				}
//				if(treeScript.currentHp == 0)
//					treeScript.playerScoring = 0;
			}
		}

		else if(Input.GetButtonDown("Wsad W") && actionBlock == false)
		{
			anim.SetTrigger("move");

			transform.rotation = Quaternion.Euler(0f, 0f, 0f);

			PlayerMovement();
		}

		else if(Input.GetButtonDown("Wsad S") && actionBlock == false)
		{
			anim.SetTrigger("move");
			transform.rotation = Quaternion.Euler(0f, 180f, 0f);
			PlayerMovement();
		}

		else if(Input.GetButtonDown("Wsad A") && actionBlock == false)
		{
			anim.SetTrigger("move");
			transform.rotation = Quaternion.Euler(0f, -90f, 0f);
			PlayerMovement();
		}
		else if(Input.GetButtonDown("Wsad D") && actionBlock == false)
		{
			anim.SetTrigger("move");
			transform.rotation = Quaternion.Euler(0f, 90f, 0f);
			PlayerMovement();
		}
		else if(verAxis.axisPressed && actionBlock == false)
		{
			Debug.Log("Cut");
			anim.SetTrigger("move");
			if(verAxis.value > 0f )
			{
				transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			}
			else
			{
				transform.rotation = Quaternion.Euler(0f, 180f, 0f);
			}
			
			PlayerMovement();
		}
		else if(horAxis.axisPressed && actionBlock == false)
		{
			//Debug.Log("    value: "+horAxis.value);
			anim.SetTrigger("move");
			if(horAxis.value > 0f )
			{
				transform.rotation = Quaternion.Euler(0f, 90f, 0f);
			}
			else
			{
				transform.rotation = Quaternion.Euler(0f, -90f, 0f);
			}
			
			PlayerMovement();
		}
		else
		{
			playerSetDown = false;

			if(treeHit)
			{
				Tree treeScript = treeHit.GetComponent<Tree>();
				if(treeScript.anim && treeScript.cutParticle)
				{
					treeScript.anim.SetBool("TreeCut", false);
					//treeScript.cutParticle.GetComponent<ParticleSystem> ().Stop();
				}
			}
			anim.SetBool("cutB", false);
		}
	}

	void PlayerMovement()
	{
		InputFrequency();

		if(CanMoveInDirection(transform.forward) && !movement)
		{
			startPosition = transform.position;
			newPosition = startPosition + transform.forward;
			
			startTime = Time.time;
			movement = true;
		}
		else if(CanMoveInDirection(transform.forward) && movement && !queueFlag)
		{
			queueFlag = true;
			queuePosition = newPosition + transform.forward;
		}
	}

	bool CanMoveInDirection(Vector3 dir)
	{
		Debug.DrawRay(rayShooter.position, dir, Color.red, 1f);
		RaycastHit hit = new RaycastHit();
		if(Physics.Raycast(rayShooter.position, dir, out hit, 1.0f, treeMask) || 
		   		Physics.Raycast(rayShooter.position, dir, out hit, 1.0f, boudriesMask) ||
		   		Physics.Raycast(rayShooter.position, dir, out hit, 1.0f, playerMask))
			return false;
		else
			return true;
	}

	GameObject HitTree(Vector3 dir)
	{
		Debug.DrawRay(rayShooter.position, dir, Color.red, 1f);
		RaycastHit hit = new RaycastHit();
		if(Physics.Raycast(rayShooter.position, dir, out hit, 1.0f, treeMask))
			return hit.collider.gameObject;
		else
			return null;
	}

	void InputFrequency()
	{
		inputHz = animBoost - lastInput;
		lastInput = animBoost;
		inputMultiplier = 1/inputHz;
	}
}
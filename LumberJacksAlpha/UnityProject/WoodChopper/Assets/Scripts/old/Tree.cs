//jak zmienić kolor koszuli tylko u jednego...

//jak sprawdzić który pierwszy ściął drzewo

//i tak źle, bo jak drzewo obok to można się odwrócić// zrobione //można się szybko odwrócić, wtedy warunek czy drzewo jest na wprost nie będzie spełniony i nie wejdzie do else i nie zrobi się stop (skrypt player)
//koniecznie random kiedy ginie inaczej żadna zabawa
//coś error po jakimś czasie

//jak się drzewo samo wyspawnuje z przodu to się pojawia, że anim has not been assigned...

//przeciwni drwale idą jedno pole w twoją stronę, chwilę czekają i znowu jedno - wtedy na nich można zrzucać drzewa.
//najlepiej multi i zrzucanie drzew na siebie. Nie wiem, jak zrobić walenie w siebie siekierami czy można czy nie, może jakieś ogłuszenie

//zwalone, że jak się drzewo przewraca a je sieknę to wstaje

//ewentualnie kopniak robi ratunek przed spadającym drzewem, nic innego tylko odbija
//klikanie na szybko może zmienić

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tree : MonoBehaviour {

	LayerMask currenPlayer;

	int playerScoring;

	public bool treeIsDead = false;

	bool applyPoints = false;

	public GameObject cutParticle;
	public GameObject dieParticle;

	bool domino = false;
	public Vector3 deathDirection;

	Vector3[] fallDirection = new Vector3[4];

	public Animator anim;

	GameObject childTree;
	float treeGrowth = 1.5f;
	float treeGrowthMinus = 0.1f;

	public LayerMask treeMask;
	public LayerMask playerMask;
	public LayerMask playerMask2;
	public Transform rayShooter;

	float currentTreePoints = 10.0f;
	float treepoints = 10.0f;
	float pointsBoost = 1.5f;

	float hp = 20.0f;
	public float currentHp = 20.0f;
	float hpBoost = 20.0f;

	float hpTimer = 5.0f;
	float currentHpTime = 0.0f;

	float currentTime = 0.0f;
	float timer = 0.1f;

	float currentGrowthTime = 0.0f;
	float growthTimer = 5.0f;
	float growthTimerMulti = 1.5f;

	bool inputBlock = false;
	float cooldown = 0.2f;
	float currentCooldown = 0.0f;
	public Coordinates treeCoords;

	bool hitDone = false;
	bool treeOld = false;
	//bool isDead = false;

	bool inCutFunction = false;
	bool isCut = false;

	GameManager manager;

	void Start ()
	{
		GameObject cutObject = (GameObject)Resources.Load ("Cut");
		cutParticle = (GameObject)GameObject.Instantiate (cutObject, new Vector3(treeCoords.i, 1.0f, treeCoords.j), Quaternion.identity);

		GameObject dieObject = (GameObject)Resources.Load ("Explode");
		dieParticle = (GameObject)GameObject.Instantiate (dieObject, new Vector3(treeCoords.i, 1.0f, treeCoords.j), Quaternion.identity);

		//cutParticle.transform.position = new Vector3 (treeCoords.i, 1.0f, treeCoords.j);
		//cutParticle.transform.localPosition = Vector3.zero;

		childTree = gameObject.transform.GetChild (0).gameObject;
		anim = childTree.GetComponent<Animator> ();

		GameObject managerObject = GameObject.Find("GameManagerObject");
		manager = managerObject.GetComponent<GameManager>();

		childTree.transform.localScale *= 0.5f;
	}

	void Update () {

		//Debug.Log (currentHp);
		if(playerScoring == 0)
			currenPlayer = playerMask;
		if(playerScoring == 1)
			currenPlayer = playerMask2;

		Grow ();

		ParticleUpdate ();

		currentTime += Time.deltaTime;
		currentCooldown += Time.deltaTime;

		if(cooldown < currentCooldown)
			inputBlock = false;

		if(currentHp <= 0)
		{
			anim.SetBool("TreeDown", true);
			DeathAnimation();
		}
		if(isCut)
		{
			Debug.Log("Cut");
		}
		else
		{
			cutParticle.GetComponent<ParticleSystem>().Stop();
		}
	}

	void ParticleUpdate()
	{
		if(Physics.Raycast(rayShooter.position, transform.forward, 1.0f, currenPlayer))
		{
			cutParticle.transform.rotation = Quaternion.Euler(0, 90, 0);
		}
		else if(Physics.Raycast(rayShooter.position, -transform.right, 1.0f, currenPlayer))
		{
			cutParticle.transform.rotation = Quaternion.Euler(0, 0, 0);
		}
		else if(Physics.Raycast(rayShooter.position, transform.right, 1.0f, currenPlayer))
		{
			cutParticle.transform.rotation = Quaternion.Euler(0, 180, 0);
		}
		else if(Physics.Raycast(rayShooter.position, -transform.forward, 1.0f, currenPlayer))
		{
			cutParticle.transform.rotation = Quaternion.Euler(0, -90, 0);
		}
		else
		{
			cutParticle.GetComponent<ParticleSystem> ().Stop ();
		}
	}

	void Cut()
	{
		inCutFunction = true;
		hitDone = true;

		//playerScoring = Player;

		Debug.Log(playerScoring);

		if(timer < currentTime && !inputBlock)
		{
			currentHp -= 10;
			currentTime = 0;
		}
		isCut = true;
		//IsNotCut();
		//StopCoroutine("NotCut");
		StartCoroutine("NotCut");
		inCutFunction = false;
	}

//	void IsNotCut()
//	{
//		if(inCutFunction == false)
//			isCut = false;
//	}

	IEnumerator NotCut()
	{
		yield return null;
		if(inCutFunction == false)
			isCut = false;
	}

	void PlayerSet(int Player)
	{
		playerScoring = Player;
	}

	void Kick()
	{
		if(currentHp < hp/2)
		{
			currentHp = 0;
		}
		inputBlock = true;
		currentCooldown = 0;
	}

	void DeathAnimation()
	{
		treeIsDead = true;

		if(Physics.Raycast(rayShooter.position, transform.forward, 1.0f, currenPlayer))
		{
			anim.SetTrigger("TreeDeadBack");
		}
		else if(Physics.Raycast(rayShooter.position, -transform.right, 1.0f, currenPlayer))
		{
			anim.SetTrigger("TreeDeadRight");
		}
		else if(Physics.Raycast(rayShooter.position, transform.right, 1.0f, currenPlayer))
		{
			anim.SetTrigger("TreeDeadLeft");
		}
		else if(Physics.Raycast(rayShooter.position, -transform.forward, 1.0f, currenPlayer))
		{
			anim.SetTrigger("TreeDead");
		}
	}

	void DeathDomino()
	{
		treeIsDead = true;
		domino = true;

		if(deathDirection == transform.forward)
			anim.SetTrigger("TreeDead");
		if(deathDirection == transform.right)
			anim.SetTrigger("TreeDeadRight");
		if(deathDirection == -transform.right)
			anim.SetTrigger("TreeDeadLeft");
		if(deathDirection == -transform.forward)
			anim.SetTrigger("TreeDeadBack");
	}

	void DeathOld()
	{
		treeIsDead = true;
		treeOld = true;

		Vector3[] direction = new Vector3[4];
		direction[0] = transform.forward;
		direction[1] = transform.right;
		direction[2] = -transform.right;
		direction[3] = -transform.forward;
		
		int randomDirection = Random.Range(0, direction.Length);

		deathDirection = direction[randomDirection];

		if(direction[randomDirection] == transform.forward)
			anim.SetTrigger("TreeDead");
		if(direction[randomDirection] == transform.right)
			anim.SetTrigger("TreeDeadRight");
		if(direction[randomDirection] == -transform.right)
			anim.SetTrigger("TreeDeadLeft");
		if(direction[randomDirection] == -transform.forward)
			anim.SetTrigger("TreeDeadBack");
	}

	public void Death()
	{
		if(domino == false)
		{
			if(treeOld == true)
			{
				DeathDomino(deathDirection);
			}

			if(Physics.Raycast(rayShooter.position, transform.forward, 1.0f, currenPlayer))
			{
				applyPoints = true;
				DeathDomino(-transform.forward);
			}
			if(Physics.Raycast(rayShooter.position, -transform.right, 1.0f, currenPlayer))
			{
				applyPoints = true;
				DeathDomino(transform.right);
			}
			if(Physics.Raycast(rayShooter.position, transform.right, 1.0f, currenPlayer))
			{
				applyPoints = true;
				DeathDomino(-transform.right);
			}
			if(Physics.Raycast(rayShooter.position, -transform.forward, 1.0f, currenPlayer))
			{
				applyPoints = true;
				DeathDomino(transform.forward);
			}
		}
		else
		{
			DeathDomino(deathDirection);
		}

		if(applyPoints == true)
			manager.Points (currentTreePoints);
		manager.treeFree.Add(treeCoords);
		manager.treePlaced.Remove(treeCoords);

		dieParticle.GetComponent<ParticleDestructor> ().dieParticlePlay ();

		//Destroy (dieParticle);
		Destroy (cutParticle);
		Destroy(gameObject);
	}

	void DeathDomino(Vector3 Direction)
	{
		//Position wyznacza tylko kierunek a miejsce skąd leci ray to określone dla drzewa
		RaycastHit hit = new RaycastHit();
		if(Physics.Raycast(rayShooter.position, Direction, out hit, 1.0f, treeMask))
		{
			Tree dominoTree = hit.collider.gameObject.GetComponent<Tree>();
			
			//jak to z domina jest current, to mniejsze może przewrócić większe jak je podgryziemy
			//zależy jak chcę, bo i tak w sumie nie widać, czy drzewo podgryzione - na razie liczy się rozmiar czyli hp
			if(dominoTree.hp <= hp && dominoTree.treeIsDead == false)
			{
				if(applyPoints == true)
					dominoTree.applyPoints = true;
				dominoTree.deathDirection = Direction;
				dominoTree.DeathDomino();
			}
		}
		if(Physics.Raycast(rayShooter.position, Direction, out hit, 1.0f, playerMask))
		{
			GameObject bloodObject = (GameObject)Resources.Load ("Blood");
			GameObject bloodParticle = (GameObject)GameObject.Instantiate (bloodObject, hit.collider.transform.position, Quaternion.identity);
			bloodParticle.GetComponent<ParticleSystem>().Play();
			Destroy(hit.collider.gameObject);
		}
		if(Physics.Raycast(rayShooter.position, Direction, out hit, 1.0f, playerMask2))
		{
			GameObject bloodObject = (GameObject)Resources.Load ("Blood");
			GameObject bloodParticle = (GameObject)GameObject.Instantiate (bloodObject, hit.collider.transform.position, Quaternion.identity);
			bloodParticle.GetComponent<ParticleSystem>().Play();
			Destroy(hit.collider.gameObject);
		}
	}

	void Grow()
	{
		//dać, że życie wzrasta liniowo, ale punktów coraz więcej 20, 30, 40 itd.
		currentGrowthTime += Time.deltaTime;
		if(growthTimer < currentGrowthTime  && !hitDone)
		{
			currentGrowthTime = 0.0f;
			childTree.transform.localScale *= treeGrowth;
			currentHp += hpBoost;
			hp += hpBoost;
			growthTimer *= growthTimerMulti;
			float randomAgeFall;
			randomAgeFall = Random.Range(0.2f, 0.5f);
			float ageFall = 1.0f + randomAgeFall;
			if(treeGrowth >= ageFall)
			{
				treeGrowth -= treeGrowthMinus;
			}
			else
			{
				DeathOld();
			}
			treepoints *= pointsBoost;
			currentTreePoints += treepoints;
		}
	}
}

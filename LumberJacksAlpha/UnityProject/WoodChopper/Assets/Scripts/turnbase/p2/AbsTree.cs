using UnityEngine;
using System.Collections.Generic;
using StaticStructure;


public abstract class AbsTree : MonoBehaviour
{
	[HideInInspector] public p2Cell cell;
	public TreeType type;
	[SerializeField] string[] plantDialogs;
	[SerializeField] string[] displayDialogs;

	public string plantLog {
		get {
			return plantDialogs[Random.Range(0,plantDialogs.Length)];
		}
	}

	public string displayLog {
		get {
			return displayDialogs[Random.Range(0,displayDialogs.Length)];
		}
	}

	public TreeActivateTime activateTime;
	AbsBuff[] buffs;
	public int plantCost=1;

	public int defaultTurnToLife = 2;
	int turnToLifeCounter;
	protected TreeState state;

	virtual protected void Start () {
		buffs = GetComponents<AbsBuff>();
	}

	float _timer;
	Quaternion fallingQuat;
	float dominoDelayTime;

	float startScale = 0.5f;
	virtual protected void Update () {
		switch (state ) {
		case TreeState.InSeed:
			break;
		case TreeState.Growing:
			_timer += Time.deltaTime;
			transform.localScale = Vector3.Lerp(Vector3.one*startScale, Vector3.one,_timer);
			if ( transform.localScale == Vector3.one ) { 
				state = TreeState.Grown;
			}
			break;
		case TreeState.Grown:
			break;
		case TreeState.WaitDomino:
			if ( dominoDelayTime < 0 ) {
				_timer = 0;
				state = TreeState.Falling;
			} else dominoDelayTime -= Time.deltaTime;
			break;
		case TreeState.Falling:
			if (Quaternion.Angle ( transform.rotation,fallingQuat ) < 1f ) {
				gameObject.SetActive(false);
				transform.rotation = Quaternion.identity;
				p2Scene.Instance.treesInScene.Remove(this);
				cell.RemoveTree();

				ActivateOnFall(choper);

				var c = cell.Get(fx,fz);
				if ( c != null ) {
					if ( c.player != null ) c.player.OnBeingChoped(choper,0);
				}
				break;
			}
			_timer += Time.deltaTime;
			transform.rotation = Quaternion.Lerp(Quaternion.identity, fallingQuat,_timer);
		
			break;
		}
	}

	virtual public void OnTurnStart (int turn_nb ) {
	}

	//---------------------------------- state query -----------------------------------------
	virtual public bool IsPassable () {
		return false;
	}
	
	virtual public bool CanBeAffectedByDomino () {
		return true;
	}

	virtual public bool PassDominoFuther () {
		return state == TreeState.Grown | state == TreeState.Growing;
	}

	//------------------------------- player messages --------------------------------------------------
	
	virtual public void OnBeingPlant (p2Player p , int deltaTurn ) {
		turnToLifeCounter = defaultTurnToLife + deltaTurn;
		p2Scene.Instance.treesInScene.Add(this);

		fx = 0; 
		fz = 0;
		state = TreeState.InSeed;
		transform.localScale = Vector3.one*startScale;
	}

	p2Player choper;

	int fx,fz;
	virtual public void OnBeingChoped (  p2Player player, p2Cell sourceCell , int tier, int acCost=0) {
		if ( state == TreeState.Falling | state == TreeState.WaitDomino ) {
			return;
		}
		fx = cell.x- sourceCell.x;
		fz = cell.z- sourceCell.z;
		choper = player;

		ActivateOnChop(player, ref fx, ref fz);

		if ( !(fx == 0 & fz == 0 ) ) {
			if ( tier == 0 ) player.OnChopDone(acCost);
			fallingQuat = Angle.Convert(fx,fz);
			dominoDelayTime = tier * p2Scene.Instance.globalDominoDelay;

			if ( PassDominoFuther() ) {
				p2Cell c;
				if ( (c= cell.Get(fx,fz) ) != null ) {
					if ( c.tree == null ? false : c.tree.CanBeAffectedByDomino()  ) {
						c.tree.OnBeingChoped(player, cell,tier+1);
					} else player.OnCredit(tier);
				} else player.OnCredit(tier);
			} else player.OnCredit(tier);

			
			state = TreeState.WaitDomino;
		}
	}

	virtual protected void ActivateOnChop (p2Player player, ref int fx, ref int fz ) {
	}

	virtual protected void ActivateOnFall (p2Player player ) {
	}

	virtual public void Activate () { 
	}


	//------------------------------ update per turn ----------------------------------------
	virtual public void OnBackgroundUpdate (List<p2Player> players ) {
		if ( turnToLifeCounter < 0 ) {
			return;
		}
		if ( turnToLifeCounter == 0 ) {
			_timer = 0;
			state = TreeState.Growing;
		}
		turnToLifeCounter --;
	}
	
	//-------------------------- used by pool ---------------------------
	virtual public bool CanBeReused () {
		if ( buffs != null ) {
			foreach ( var b in buffs ) {
				if ( b.IsBeingUsed() ) return false;
			}
		}
		return !isActiveAndEnabled;
	}
}


using UnityEngine;
using System.Collections.Generic;
using StaticStructure;


public abstract class p3AbsTree : MonoBehaviour
{
	[HideInInspector] public p3Cell cell;
	[SerializeField] protected Transform graphicalModel;
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
	public bool canBePlantedByPlayer;
	public TreeActivateTime activateTime;
	AbsBuff[] buffs;
	public int plantCost=1;
	
	public int defaultTurnToLife = 2;
	protected int turnToLifeCounter;
	protected TreeState state;
	
	protected Vector3 originScale;
	
	virtual protected void Awake () {
		originScale = graphicalModel.localScale;
		startScale = originScale * 0.5f;
	}
	virtual protected void Start () {
		buffs = GetComponents<AbsBuff>();
	}
	
	float _timer;
	Quaternion fallingQuat;
	float dominoDelayTime;
	
	Vector3 startScale;
	virtual protected void Update () {
		switch (state ) {
		case TreeState.InSeed:
			break;
		case TreeState.Growing:
			_timer += Time.deltaTime;
			graphicalModel.localScale = Vector3.Lerp(startScale, originScale,_timer);
			if ( graphicalModel.localScale == originScale ) {
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
			if (Quaternion.Angle ( graphicalModel.rotation,fallingQuat ) < 1f ) {
				gameObject.SetActive(false);
				graphicalModel.rotation = Quaternion.identity;
				p3Scene.Instance.treesInScene.Remove(this);
				cell.RemoveTree();
				cell.HighLightOn(false);
				cell.SelectedOn(false);
				cell.AuraOn(false);
				ActivateOnFall(choper);
				
				if ( dealDmg ) {
					var c = cell.Get(fx,fz);
					if ( c != null ) {
						if ( c.player != null ) c.player.OnBeingChoped(choper,0);
					}
				}
				break;
			}
			_timer += Time.deltaTime;
			graphicalModel.rotation = Quaternion.Lerp(Quaternion.identity, fallingQuat,_timer);
			
			break;
		}
	}
	
	bool dealDmg = false;
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
	
	virtual public void OnTouchEnter () {
		p3Ui.Instance.ingamePanel.DisplayDialog(displayLog);
		
	}
	
	virtual public void OnTouchExit () {
	}
	
	virtual public void OnBeingPlant (p3Player p , int deltaTurn ) {
		turnToLifeCounter = defaultTurnToLife + deltaTurn;
		p3Scene.Instance.treesInScene.Add(this);
		
		fx = 0;
		fz = 0;
		dealDmg = false;
		state = TreeState.InSeed;
		graphicalModel.localScale = startScale;
	}
	
	p3Player choper;
	
	int fx,fz;
	virtual public void OnBeingChoped (  p3Player player, p3Cell sourceCell , int tier, int acCost=0) {
		if ( state == TreeState.Falling | state == TreeState.WaitDomino ) {
			return;
		}
		fx = cell.x- sourceCell.x;
		fz = cell.z- sourceCell.z;
		choper = player;
		
		ActivateOnChop(player, ref fx, ref fz);
		
		if ( tier ==  0 ) player.OnChopDone(acCost);

		if ( !(fx == 0 & fz == 0 ) ) {
			fallingQuat = Angle.Convert(fx,fz);
			dominoDelayTime = tier * p3Scene.Instance.globalDominoDelay;
			
			if ( PassDominoFuther() ) {
				p3Cell c;
				if ( (c= cell.Get(fx,fz) ) != null ) {
					if ( c.tree == null ? false : c.tree.CanBeAffectedByDomino()  ) {
						c.tree.OnBeingChoped(player, cell,tier+1);
					} else player.OnCredit(tier);
				} else player.OnCredit(tier);
			} else {
				Debug.Log("state == TreeState.InSeed ? " + (state == TreeState.InSeed));
				player.OnCredit(state == TreeState.InSeed? tier-1 : tier);
			}
			
			dealDmg = state != TreeState.InSeed;
			state = TreeState.WaitDomino;
		} 
	}
	
	virtual protected void ActivateOnChop (p3Player player, ref int fx, ref int fz ) {
	}
	
	virtual protected void ActivateOnFall (p3Player player ) {
	}
	
	virtual public bool Activate () {
		if ( state == TreeState.InSeed ) return false;
		return true;
	}
	
	
	//------------------------------ update per turn ----------------------------------------
	virtual public void OnBackgroundUpdate (List<p3Player> players ) {
		if ( state == TreeState.InSeed ) {
			if ( turnToLifeCounter == 0 ) {
				_timer = 0;
				state = TreeState.Growing;
				cell.AuraOn(false);
			}
			
			if ( turnToLifeCounter == 1 ) {
				cell.AuraOn(true);
			}
			turnToLifeCounter --;
		}
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
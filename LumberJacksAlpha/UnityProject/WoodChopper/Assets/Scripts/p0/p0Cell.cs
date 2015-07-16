using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class p0Cell : MonoBehaviour {
	/* -2 == tree; -1 == free , -3 == reserved*/
	private int _locked;
	public int locked { 
		get {
			return _locked;
		}
		set {
			switch ( value ) {
			case -3: 
				ground.color = p0Const.Instance.cellSettings.reservedForTree;
				freeList.Remove(this);
				break;
			case -1: 
				ground.color = p0Const.Instance.cellSettings.free;
				freeList.Add(this);
				break;
			case -2: 
				ground.color = p0Const.Instance.cellSettings.free;
				freeList.Remove(this);
				break;
			default :
				ground.color = p0Const.Instance.cellSettings.reservedForPlayer;
				break;
			}
			_locked = value;
		}
	}
	[HideInInspector] public int x,z;
	[HideInInspector] public Vector3 position;
	[HideInInspector] public List<p0Cell> freeList;
	public MeshRenderer tree;
	public SpriteRenderer ground;
	public Animator treeAnimator;

	static int fxHash = Animator.StringToHash("fx");
	static int fzHash = Animator.StringToHash("fz");

	p0Const _const;

	void Start () {
		treeAnimator.gameObject.SetActive(false);
		_const = p0Const.Instance;
	}

	public void OnSelectMode () {
		ground.color = _const.cellSettings.availableInSelectMode;
	}
	public void OnNotSelectMode () {
		locked = _locked;
	}

	public void OnSelected () {
		ground.color = _const.cellSettings.selectedInSelectMode;
	}

	public bool CanStepOn () {
		return locked == -1 | locked == -3;
	}

	public bool CanPlantTreeOn() {
		return locked == -1;
	}

	public bool CanChop () {
		return locked == -2;
	}

	public void ChopTree (int id, int _fx, int _fz, int tree_nb) {
		if ( _d == null & treeAnimator.isActiveAndEnabled ) {
			treeAnimator.SetInteger(fxHash,_fx);
			treeAnimator.SetInteger(fzHash,_fz);

			StartCoroutine(_d = Disapear(id, _fx,_fz,tree_nb));
		} else if ( tree_nb > 0 ) p0CellController.Instance.GetPlayer(id).CreditPoints((int)Mathf.Pow(tree_nb,2));
	}

	IEnumerator Disapear(int id, int fallx, int fallz, int tree_nb) {
		yield return new WaitForSeconds(_const.treeSettings.dominoDelay);
		var c = Get(fallx,fallz);
		if ( c != null ) {
			c.ChopTree(id,fallx,fallz,tree_nb+1);
			foreach ( var p in p0CellController.Instance.players ) {
				if ( p.netview.isMine & p.IsOnCell ( c.x, c.z ) ) {
					p.OnLostHp(_const.gameplaySettings.treeFallDamage);
				}
			}
		} else if ( tree_nb > 0 ) p0CellController.Instance.GetPlayer(id).CreditPoints((int)Mathf.Pow(tree_nb+1,2));
		yield return new WaitForSeconds(_const.treeSettings.additiveDisapearDelay);
		locked = -1;
		treeAnimator.transform.rotation = Quaternion.identity;
		treeAnimator.gameObject.SetActive(false);
		_d = null;
	}

	public void OnGenTreeReserved () {
		ground.color = _const.cellSettings.reservedForTree;
		locked = -3;
	}

	IEnumerator _d;

	static int growHash = Animator.StringToHash("grow");

	public void PlantTree() {
		if ( _d == null ) { 
			locked = -2;
			treeAnimator.gameObject.SetActive(true);
			treeAnimator.SetInteger(fxHash,0);
			treeAnimator.SetInteger(fzHash,0);
		}
	}

	public void Free() {
		locked = -1;
	}

	public p0Cell[,] map { get; private set; }

	/* x,z must be -1,0,1 or else ArrayOutOfBounds Exception */
	public void Map (int x, int z, p0Cell p ) {
		if( map == null ) {
			map = new p0Cell[3,3];
			map[1,1] = this;
		}
		map[x+1,z+1] = p;
	}

	public p0Cell Get(int x,int z ) {
		return map[x+1,z+1];
	}
}


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(PhotonView))]
public class p1Player : MonoBehaviour
{
	enum PlayerState { OnMove, WaitConfirm, OnChop, OnPlant }
	private PlayerState _state;
	PlayerState state {
		set {
			lastState = _state;
			_state = value;
		}
		get {
			return _state;
		}
	}

	PhotonView netview;
	p1Const _const;
	GuiInputAdapter guiInput;

	p1Map map;

	void Start () {
		netview = GetComponent<PhotonView>();
		_const = p1Const.Instance;
		guiInput = GuiInputAdapter.Instance;
		map = p1Map.Instance;
	}

	public bool IsPassable() {
		return false;
	}

	bool _run;

	void Update () {
		if ( _run & netview.isMine ) {
			_UpdateState ();
		} else {
		}
	}

	p1Cell currentCell;

	p1Direction.DIR2 ShowCanPlantCells () {
		p1Direction.DIR2 deltaSelectedCell = null;
		p1Cell pointed_cell = null;
		pointed_cell = map.GetPointedCell (Camera.main.ScreenToWorldPoint(Input.mousePosition));
		if ( pointed_cell != null ) {
			var selectable = Mathf.Abs( pointed_cell.x - currentCell.x ) < 2 & Mathf.Abs( pointed_cell.z - currentCell.z ) < 2;
			var m = currentCell.map;
			foreach ( var c in m ) {
				if ( c == null ) continue;
				if ( c.CanPlant() ) {
					if ( c == pointed_cell ) {
						c.Highlight(_const.cellSettings.selectedInSelectMode);
						deltaSelectedCell = new p1Direction.DIR2(c.x - currentCell.x, c.z - currentCell.z);
					} else {
						c.Highlight(_const.cellSettings.availableInSelectMode);
					}
				}
			}
		}
		return deltaSelectedCell;
	}

	void HideAvailableCells () {
		var m = currentCell.map;
		foreach ( var c in m ) {
			if ( c == null ) continue;
			c.OffHighlight ();
		}
	}
		
	p1Direction.DIR2 ShowCanChopCells () {
		p1Direction.DIR2 deltaSelectedCell = null;
		p1Cell pointed_cell = null;
		pointed_cell = map.GetPointedCell (Camera.main.ScreenToWorldPoint(Input.mousePosition));
		if ( pointed_cell != null ) {
			var m = currentCell.map;
			foreach ( var c in m ) {
				if ( c == null ) continue;
				if ( c == currentCell ) continue;
				if ( c.CanChop () ) {
					if ( c == pointed_cell ) {
						c.Highlight(_const.cellSettings.selectedInSelectMode);
						deltaSelectedCell = new p1Direction.DIR2(c.x - currentCell.x, c.z - currentCell.z);
						/* NEED: hide ui cancel button */
					} else {
						c.Highlight(_const.cellSettings.availableInSelectMode);
					}
				}
			}
		}
		return deltaSelectedCell;
	}

	p1Direction.DIR2 ShowCanStepCells () {
		p1Direction.DIR2 deltaSelectedCell = null;
		p1Cell pointed_cell = null;
		pointed_cell = map.GetPointedCell (Camera.main.ScreenToWorldPoint(Input.mousePosition));
		if ( pointed_cell != null ) {
			var m = currentCell.map;
			foreach ( var c in m ) {
				if ( c == null ) continue;
				if ( c == currentCell ) continue;
				if ( c.CanStep () ) {
					if ( c == pointed_cell ) {
						c.Highlight(_const.cellSettings.selectedInSelectMode);
						deltaSelectedCell = new p1Direction.DIR2(c.x - currentCell.x, c.z - currentCell.z);
					} else {
						c.Highlight(_const.cellSettings.availableInSelectMode);
					}
				}
			}
		}
		return deltaSelectedCell;
	}

	PlayerState lastState;
	p1Direction.DIR2 deltaCell;

	void _UpdateState () {
		switch (state ) {
		case PlayerState.OnMove : 
			if ( guiInput.HasTreeSelected()  ) {
				state = PlayerState.OnPlant;
				/* display uiCancel button */
			} else 
			if ( guiInput.HashChopSelected() ) {
				state = PlayerState.OnChop;
				/* display uiCancel button */
			} else {
				deltaCell = ShowCanStepCells ();
				if ( deltaCell != null ) {
					state = PlayerState.WaitConfirm;
				}
			}
			break;
		case PlayerState.OnChop:
			if ( guiInput.IsCanceled() ) {
				state = PlayerState.OnMove;
				guiInput.FlushCancel();
			} else 
			if ( guiInput.HasTreeSelected() ) {
				state = PlayerState.OnPlant;
				guiInput.FlushChopButton();
			} else {
				deltaCell = ShowCanChopCells ();
				if ( deltaCell != null ) {
					state = PlayerState.WaitConfirm;
				}
			}
			break;
		case PlayerState.OnPlant:
			if ( guiInput.IsCanceled() ) {
				state = PlayerState.OnMove;
				guiInput.FlushCancel();
			} else	
			if ( guiInput.HashChopSelected() ) {
				guiInput.FlushTree();	
				state = PlayerState.OnChop;
			} else {
				deltaCell = ShowCanPlantCells ();
				if ( deltaCell != null ) {
					state = PlayerState.WaitConfirm;
				}
			}
			break;
		case PlayerState.WaitConfirm:
			if ( guiInput.IsApproved() ) {
				if ( lastState == PlayerState.OnPlant ) {
					netview.RPC("Plant",PhotonTargets.All, p1Direction.Encode(deltaCell));
				}
				else if ( lastState == PlayerState.OnChop ) {
					// DO CHOP
				}
				else if ( lastState == PlayerState.OnMove ) {
					// DO MOVE
				}
				state = PlayerState.OnMove;
			} else {
				if ( lastState == PlayerState.OnPlant ) state = PlayerState.OnPlant;
				else if ( lastState == PlayerState.OnChop ) state =  PlayerState.OnChop;
				else if ( lastState == PlayerState.OnMove ) state = PlayerState.OnMove;
			}
			break;
		}
	}

	[RPC] void Plant ( byte dir1, byte p1TreeType ) {
		int fx,fz;
		p1Direction.Decode(dir1, out fx, out fz);
	}

	[RPC] void Chop ( byte dir1 ) {
		int fx,fz;
		p1Direction.Decode(dir1, out fx, out fz);
	}
}

public class p1Direction {
	public enum DIR1 : byte { 
		d00 = 0,
		d01 = 1,
		d10 = 2,
		d11 = 3,
		d0_1 = 4,
		d_10 = 5,
		d_1_1 = 6,
		d1_1 = 7,
		d_11 = 8
	}

	static DIR1[,] encode;
	static DIR2[] decode;

	static p1Direction () {
		encode = new DIR1[3,3];
		decode = new DIR2[9];

		encode[0,0] = DIR1.d_1_1;
		encode[0,1] = DIR1.d_10;
		encode[0,2] = DIR1.d_11;
		encode[1,0] = DIR1.d0_1;
		encode[1,1] = DIR1.d00;
		encode[1,2] = DIR1.d01;
		encode[2,0] = DIR1.d1_1;
		encode[2,1] = DIR1.d10;
		encode[2,2] = DIR1.d11;

		decode[0] = new DIR2(0,0);
		decode[1] = new DIR2(0,1);
		decode[2] = new DIR2(1,0);
		decode[3] = new DIR2(1,1);
		decode[4] = new DIR2(0,-1);
		decode[5] = new DIR2(-1,0);
		decode[6] = new DIR2(-1,-1);
		decode[7] = new DIR2(1,1);
		decode[8] = new DIR2(-1,1);
	}

	public class DIR2 {
		public DIR2 (int _fx, int _fz ) { fx = _fx; fz = _fz; }
		public int fx,fz;
	}

	public static void Decode ( byte dir1 , out int fx, out int fz) {
		var l = decode[dir1];
		fx = l.fx;
		fz = l.fz;
	}
	
	/* fx,fz must be -1,0,1 or else ArrayOutOfBounds Exception */
	public static byte Encode ( DIR2 dir2 ) {
		return (byte)encode[dir2.fx-1,dir2.fz-1];
	}
}


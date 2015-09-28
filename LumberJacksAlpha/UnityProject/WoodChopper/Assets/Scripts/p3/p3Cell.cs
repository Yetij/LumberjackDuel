using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class p3Cell : MonoBehaviour, AbsServerObserver {
	[HideInInspector] public int x,z;
	
	public static List<p3Cell > free = new List<p3Cell>();
	
	public GameObject highlight;
	public GameObject selected;
	public GameObject aura;
	
	p3AbsTree _tree;
	public p3AbsTree tree { 
		get {
			return _tree;
		}
		private set {
			if ( value == null & _player == null) { 
				if ( !free.Contains(this) ) free.Add(this);
			} else if ( free.Contains(this) ) free.Remove(this);
			_tree = value;
		}
	}
	p3Player _player;
	public p3Player player { 
		get {
			return _player;
		}
		private set {
			if ( value == null & tree == null) { 
				if ( !free.Contains(this) ) free.Add(this);
			} else if ( free.Contains(this) ) free.Remove(this);
			_player = value;
		}
	}
	
	public void OnPlayerMoveIn ( p3Player player ) {
		player.transform.position = transform.position;
		player.currentCell = this;
		this.player = player;
	}
	
	public void OnPlayerMoveOut ( ) {
		this.player = null;
	}
	
	public void OnTurnStart ( int turn_nb ){
	}
	
	public void OnBackgroundStart ( ){
	}
	
	public bool CanMoveTo () {
		return ( player == null ? true: player.IsPassable() ) & ( tree == null ? true: tree.IsPassable() );
	}
	
	public bool CanPlant () {
		return player == null & tree == null;
	}
	
	public bool CanChop () {
		return player != null | ( tree == null ? false: !tree.IsPassable() );
	}
	
	public void OnPlayerPlantTree (p3AbsTree t, p3Player p,  int deltaTurn) {
		tree = t;
		tree.OnBeingPlant(p, deltaTurn);
		
		t.transform.position = transform.position;
		t.cell = this;
	}
	
	public void Reset () {
		if ( tree != null ) tree.gameObject.SetActive(false);
		tree = null;
		player = null;
		highlight.SetActive(false);
		selected.SetActive(false);
	}
	
	public void RemoveTree() {
		tree = null;
	}
	
	public void HighLightOn (bool t) {
		highlight.SetActive(t);
	}
	
	public void AuraOn (bool t ) {
		aura.SetActive(t);
	}
	public void SelectedOn (bool t) {
		selected.SetActive(t);
	}
	
	
	public p3Cell[,] map { get; private set; }
	
	/* x,z must be -1,0,1 or else ArrayOutOfBounds Exception */
	public void Link (int x, int z, p3Cell p ) {
		if( map == null ) {
			map = new p3Cell[3,3];
			map[1,1] = this;
		}
		map[x+1,z+1] = p;
	}
	
	public p3Cell Get(int x,int z ) {
		return map[x+1,z+1];
	}
}

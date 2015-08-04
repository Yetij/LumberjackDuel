﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class p2Cell : MonoBehaviour, AbsServerObserver {
	[HideInInspector] public int x,z;
	[HideInInspector] public Vector3 position;
	
	public GameObject highlight;
	public GameObject selected;

	[HideInInspector] public AbsTree tree;
	[HideInInspector] public p2Player player;

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
	
	public void AddTree (AbsTree t) {
		tree = t;
		
		t.transform.position = position;
		t.cell = this;
	}

	public void HighLightOn (bool t) {
		highlight.SetActive(t);
	}
	
	public void SelectedOn (bool t) {
		selected.SetActive(t);
	}

	public void OnChop () {
	}

	public p2Cell[,] map { get; private set; }
	
	/* x,z must be -1,0,1 or else ArrayOutOfBounds Exception */
	public void Link (int x, int z, p2Cell p ) {
		if( map == null ) {
			map = new p2Cell[3,3];
			map[1,1] = this;
		}
		map[x+1,z+1] = p;
	}
	
	public p2Cell Get(int x,int z ) {
		return map[x+1,z+1];
	}
}

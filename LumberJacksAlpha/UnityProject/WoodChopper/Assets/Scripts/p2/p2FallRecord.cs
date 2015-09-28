using UnityEngine;
using System.Collections;

public class p2FallRecord 
{
	public AbsTree tree;
	public int initOrder;
	public int deltaTurn;
	public p2FallRecord ( AbsTree t, int dt, int io ) { tree = t; deltaTurn = dt; initOrder = io; }
}


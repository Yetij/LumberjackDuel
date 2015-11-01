using UnityEngine;
using System.Collections;
using System;

public enum PLAY : int {  ER1 = 0 , ER2 = 1 }
[RequireComponent(typeof(PhotonView))]
public class Lumberjack : Photon.MonoBehaviour {
    public Cell currentCell { get; private set; }
    public PLAY player;
    public LumberJackParams parameters;

    Server server;
    void Start() {
        server = GameObject.FindObjectOfType<Server>();
    }

    public void MoveTo (Cell target )
    {

    }

    public void Chop (Cell target )
    {

    }

    public void Plant (Cell target )
    {

    }

    internal void VisualBeingChoped(PLAY player)
    {
        throw new NotImplementedException();
    }

    internal void VisualChop(int cx, int cy)
    {
        throw new NotImplementedException();
    }

    internal void VisualPlant(int cx, int cy)
    {
        throw new NotImplementedException();
    }

    internal void VisualMoveTo(int cx, int cy)
    {
        throw new NotImplementedException();
    }
} 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class p1TreePool : MonoBehaviour
{
	public int coreAoeNb = 2;
	public int coreBasicNb = 32;
	public int corePlusAc1Nb = 2;
	public int coreTeleNb = 4;

	List<p1CoreAoe> core_aoes;
	List<p1CoreBasic> core_basics;
	List<p1CorePlusAc1> core_plusac1s;
	List<p1CoreTeleportational> core_teles;

//	void Awake () {
//		var aoe = (GameObject ) Resources.Load("CoreAoe");
//		core_aoes = new List<p1CoreAoe>(coreAoeNb);
//		for(int i =0;i < coreAoeNb ; i++ ) {
//			var g = Instantiate(aoe);
//			g.transform.parent = gameObject.transform;
//			core_aoes.Add(g);
//		}
//
//	}
//



	private static p1TreePool _instance;
	public static p1TreePool Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p1TreePool)) as p1TreePool;
				if ( _instance == null ) throw new UnityException("Object of type p1TreePool not found");
			}
			return _instance;
		}
	}



}


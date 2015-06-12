using UnityEngine;
using System.Collections.Generic;

public class v4Pool : MonoBehaviour 
{
	private static v4Pool _instance;
	public static v4Pool Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(v4Pool)) as v4Pool;
				if ( _instance == null ) throw new UnityException("Object of type v4Pool not found");
			}
			return _instance;
		}
	}

	List<v4Tree> pool;
	GameObject tree_pref;

	public void Initialize (int init_cap=16) {
		if( tree_pref == null )
			tree_pref = (GameObject  ) Resources.Load(v4ConstValue.Instance.prefabNames._Tree);
		pool = new List<v4Tree> (init_cap);
		for(int i=0;i < init_cap; i++ ) {
			GameObject t = (GameObject ) Instantiate(tree_pref,Vector3.zero,Quaternion.identity);
			t.transform.parent = transform;
			t.SetActive(false);
			pool.Add(t.GetComponent<v4Tree>());
		}
	}

	public v4Tree Get () { 
		foreach ( v4Tree m in pool ) {
			if ( !m.isActiveAndEnabled ) {
				m.gameObject.SetActive(true);
				m.Reset();
				return m;
			}
		}
		GameObject t = (GameObject ) Instantiate(tree_pref,Vector3.zero,Quaternion.identity);
		t.transform.parent = transform;
		var _t = t.GetComponent<v4Tree>();
		pool.Add(_t);
		return _t;
	}
	public void Return(v4Tree t ) {}
}


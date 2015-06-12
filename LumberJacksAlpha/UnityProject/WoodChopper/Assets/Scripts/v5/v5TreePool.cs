using UnityEngine;
using System.Collections.Generic;

public class v5TreePool : MonoBehaviour
{
	private static v5TreePool _instance;
	public static v5TreePool Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(v5TreePool)) as v5TreePool;
				if ( _instance == null ) throw new UnityException("Object of type v5TreePool not found");
			}
			return _instance;
		}
	}
	public int init_cap=16;

	List<v5Tree> pool;
	GameObject tree_pref;
	
	void Initialize () {
		if( tree_pref == null )
			tree_pref = (GameObject  ) Resources.Load(v5Const.Instance.prefabNames._Tree);
		pool = new List<v5Tree> (init_cap);
		for(int i=0;i < init_cap; i++ ) {
			GameObject t = (GameObject ) Instantiate(tree_pref,Vector3.zero,Quaternion.identity);
			t.transform.parent = transform;
			t.SetActive(false);
			pool.Add(t.GetComponent<v5Tree>());
		}
	}

	void Start () {
		Debug.Log("tree pool start");
		Initialize();
	}

	public v5Tree Get () { 
		foreach ( v5Tree m in pool ) {
			if ( !m.isActiveAndEnabled ) {
				m.Reset();
				m.gameObject.SetActive(true);
				return m;
			}
		}
		GameObject t = (GameObject ) Instantiate(tree_pref,Vector3.zero,Quaternion.identity);
		t.transform.parent = transform;
		var _t = t.GetComponent<v5Tree>();
		_t.Reset();
		pool.Add(_t);
		return _t;
	}
	public void Return(v5Tree t ) {}
}


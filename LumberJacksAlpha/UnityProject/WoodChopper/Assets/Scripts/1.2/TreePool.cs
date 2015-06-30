using UnityEngine;
using System.Collections.Generic;

public class TreePool : MonoBehaviour
{
	private static TreePool _instance;
	public static TreePool Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(TreePool)) as TreePool;
				if ( _instance == null ) throw new UnityException("Object of type TreePool not found");
			}
			return _instance;
		}
	}
	public int init_cap=16;
	
	List<Tree> pool;
	GameObject tree_pref;
	
	void Initialize () {
		if( tree_pref == null )
			tree_pref = (GameObject  ) Resources.Load(v5Const.Instance.prefabNames._Tree);
		pool = new List<Tree> (init_cap);
		for(int i=0;i < init_cap; i++ ) {
			GameObject t = (GameObject ) Instantiate(tree_pref,Vector3.zero,Quaternion.identity);
			t.transform.parent = transform;
			t.SetActive(false);
			pool.Add(t.GetComponent<Tree>());
		}
	}
	
	void Awake () {
		Initialize();
	}
	
	public Tree Get () { 
		foreach ( Tree m in pool ) {
			if ( !m.isActiveAndEnabled ) {
				m.Reset();
				m.gameObject.SetActive(true);
				return m;
			}
		}
		GameObject t = (GameObject ) Instantiate(tree_pref,Vector3.zero,Quaternion.identity);
		t.transform.parent = transform;
		var _t = t.GetComponent<Tree>();
		_t.Reset();
		pool.Add(_t);
		return _t;
	}
	public void Return(Tree t ) {}
}


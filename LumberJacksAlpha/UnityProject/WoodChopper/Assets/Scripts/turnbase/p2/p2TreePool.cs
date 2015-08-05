using UnityEngine;
using System.Collections.Generic;


public class p2TreePool : MonoBehaviour
{
	Dictionary<p2TreeType,List<AbsTree>> pool;

	p2PoolInfo[] seeds;
	void Awake () {
		pool = new Dictionary<p2TreeType,List<AbsTree>>();

		seeds = GetComponents<p2PoolInfo>();
		for ( int i = 0; i < seeds.Length; i ++ ) {
			GameObject g = new GameObject(seeds[i].type.ToString());
			g.transform.parent = gameObject.transform;

			var l = new List<AbsTree>(seeds[i].init_cap);
			for(int k = 0; k < seeds[i].init_cap; k ++ ) {
				var t = Instantiate(seeds[i].prefab);
				t.transform.parent = g.transform;
				l.Add(t);
				t.gameObject.SetActive(false);
			}

			pool.Add(seeds[i].type, l);
		}
	}	

	public AbsTree Get(p2TreeType type) {
		List<AbsTree> subPool = null;
		pool.TryGetValue(type, out subPool);

		if ( subPool != null ) {
			foreach ( AbsTree m in subPool ) {
				if ( !m.isActiveAndEnabled ) {
					m.gameObject.SetActive(true);
					return m;
				}
			}
			foreach ( var s in seeds ) {
				if ( type == s.type ) {
					var t = Instantiate(s.prefab);
					t.transform.parent = subPool[0].transform.parent;
					var _t = t.GetComponent<AbsTree>();
					subPool.Add(_t);
					return _t;
				}
			}
		}
		throw new UnityException("Invalid tree type : " + type);
	}

	private static p2TreePool _instance;
	public static p2TreePool Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p2TreePool)) as p2TreePool;
				if ( _instance == null ) throw new UnityException("Object of type p2TreePool not found");
			}
			return _instance;
		}
	}

}


using UnityEngine;
using System.Collections.Generic;
using StaticStructure;


public class p3TreePool : MonoBehaviour
{
	Dictionary<TreeType,List<p3AbsTree>> pool;
	
	[SerializeField] p3PoolInfo[] seeds;
	
	void Awake () {
		pool = new Dictionary<TreeType,List<p3AbsTree>>();
		
		for ( int i = 0; i < seeds.Length; i ++ ) {
			GameObject g = new GameObject(seeds[i].type.ToString());
			g.transform.parent = gameObject.transform;
			
			var l = new List<p3AbsTree>(seeds[i].init_cap);
			for(int k = 0; k < seeds[i].init_cap; k ++ ) {
				var t = Instantiate(seeds[i].prefab);
				t.transform.parent = g.transform;
				l.Add(t);
				t.gameObject.SetActive(false);
			}
			
			pool.Add(seeds[i].type, l);
		}
	}	
	
	public string GetTreePlantLog (TreeType type  ) {
		foreach ( var s in seeds ) {
			if ( type == s.type ) {
				return s.prefab.plantLog;
			}
		}
		throw new UnityException("Invalid tree type");
	}
	
	public int GetTreePlantCost (TreeType type ) {
		foreach ( var s in seeds ) {
			if ( type == s.type ) {
				return s.prefab.plantCost;
			}
		}
		throw new UnityException("Invalid tree type");
	}
	
	public p3AbsTree Get(TreeType type) {
		List<p3AbsTree> subPool = null;
		pool.TryGetValue(type, out subPool);
		
		if ( subPool != null ) {
			foreach ( p3AbsTree m in subPool ) {
				if ( m.CanBeReused() ) {
					m.gameObject.SetActive(true);
					return m;
				}
			}
			foreach ( var s in seeds ) {
				if ( type == s.type ) {
					var t = Instantiate(s.prefab);
					t.transform.parent = subPool[0].transform.parent;
					var _t = t.GetComponent<p3AbsTree>();
					subPool.Add(_t);
					return _t;
				}
			}
		}
		throw new UnityException("Invalid tree type : " + type);
	}
	
	private static p3TreePool _instance;
	public static p3TreePool Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p3TreePool)) as p3TreePool;
				if ( _instance == null ) throw new UnityException("Object of type p3TreePool not found");
			}
			return _instance;
		}
	}
	
}


using UnityEngine;
using System.Collections;

public class v2MapManager : MonoBehaviour
{
	#region hiden
	private static v2MapManager _instance;
	public static v2MapManager Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(v2MapManager)) as v2MapManager;
				if ( _instance == null ) throw new UnityException("Object of type v2TreeManager not found");
			}
			return _instance;
		}
	}
	#endregion

	public GameObject tree_pref;
	public Vector3 grid_root;
	public float x_grid_offset;
	public float z_grid_offset;
	public int total_tree_x;
	public int total_tree_z;

	[SerializeField] float gen_rate = 2;
	[SerializeField] int gen_min=2;
	[SerializeField] int gen_max=6;
	[SerializeField] float domino_fall_delay=0.3f;

	v2Tree[,] tree_list;

	#region tmp/help variables , functions
	int[] no_trees = new int[] { -1, -1, -1 , -1 };
	int[] _rand = new int[]{ 1, -1 };
	int max_try_per_tree = 6;
	Vector3 _tmp = Vector3.zero;
	int last_chopped_x=-1,last_chopped_z;

	bool IsIndexValid (int x, int z ) {
		return x >=0 & z >=0 & x < total_tree_x & z < total_tree_z;
	}

	bool canPlaceTree ( int x, int z ) {
		if ( no_trees[0] != -1 ) {
			if( no_trees[0] == x & no_trees[1] == z ) return false;
		}
		if ( no_trees[2] != -1 ) {
			if( no_trees[2] == x & no_trees[3] == z ) return false;
		}
		return true;
	}

	IEnumerator _GenTree () {
		yield return null;
		while ( true ) {
			int gen_nb = Random.Range(gen_min, gen_max+1 );
			while ( gen_nb > 0 ) {
				gen_nb --;
				int x = Random.Range(0,total_tree_x);
				int z = Random.Range(0,total_tree_z);
				if ( canPlaceTree(x,z ) ) {
					if ( !tree_list[x,z].isActiveAndEnabled ) {
						tree_list[x,z].gameObject.SetActive(true);
					}
				} else {
					int vx = _rand[Random.Range(0,2)];
					int vz = _rand[Random.Range(0,2)];
					for (int i=0;i < max_try_per_tree; i++ ) {
						x += vx;
						z += vz;
						if ( x >= total_tree_x | x < 0 | z >= total_tree_z | z < 0 ) break;
						if( canPlaceTree(x,z ) ) {
							if ( !tree_list[x,z].isActiveAndEnabled ) {
								tree_list[x,z].gameObject.SetActive(true);
								break;
							}
						}
					}
				}
			}
			yield return new WaitForSeconds(gen_rate);
		}
	}
	#endregion

	void Awake () {
		tree_list = new v2Tree[total_tree_x,total_tree_z];
		for (int i=0; i < total_tree_z; i ++ ) {
			for (int j=0; j < total_tree_x; j ++ ) {
				GameObject g = (GameObject) Instantiate(tree_pref,Vector3.zero, Quaternion.identity);
				g.transform.parent = transform;
				g.transform.position = new Vector3(j*x_grid_offset+x_grid_offset/2+ grid_root.x,
				                                   0,
				                                   i*z_grid_offset+z_grid_offset/2 + grid_root.z);
				g.SetActive(false);
				tree_list[j,i] = g.GetComponent<v2Tree>();
				tree_list[j,i].SetIndex(j,i);
			}
		}
		StartCoroutine(_GenTree());
	}

	public Vector3 MoveTo (int x, int z ) {
		_tmp.Set(grid_root.x+x*x_grid_offset + x_grid_offset/2, 0, grid_root.z + z*z_grid_offset + z_grid_offset/2 );
		return _tmp;
	}


	public void OnPlayerChop (float strength, int x, int facing_x, int z, int facing_z ) {
		last_chopped_x = x+facing_x;
		last_chopped_z = z+facing_z;
		if ( !IsIndexValid(last_chopped_x,last_chopped_z) ) { 
			last_chopped_x = -1;
			return;
		}
		if ( tree_list[last_chopped_x,last_chopped_z].isActiveAndEnabled ) {
			tree_list[last_chopped_x,last_chopped_z].OnBeingChopped(strength, facing_x, facing_z );
		}
	}

	public void OnPlayerNotChop () {
		if ( last_chopped_x != -1 ) {
			if ( tree_list[last_chopped_x,last_chopped_z].isActiveAndEnabled )
				tree_list[last_chopped_x,last_chopped_z].OnNotBeingChopped();
		}
		last_chopped_x = -1;
	}

	public void OnPlayerKick ( int x, int facing_x, int z, int facing_z ) {
		int x2 = x + facing_x;
		int z2 = z + facing_z;
		if ( !IsIndexValid(x2,z2) ) return;
		if ( tree_list[x+facing_x,z+facing_z].isActiveAndEnabled ) {
			tree_list[x+facing_x,z+facing_z].OnBeingKicked(facing_x,facing_z);
		}
	}

	public bool OnPlayerMove ( int x, int facing_x, int z, int facing_z  ) {
		no_trees[0] = x;
		no_trees[1] = z;

		var x2 = x+facing_x;
		var z2 = z+facing_z;
		if ( !IsIndexValid(x2,z2) ) {
			no_trees[2] = -1;
			no_trees[3] = -1;
			return false;
		}
		bool k = !tree_list[x2,z2].isActiveAndEnabled;
		if ( k ) {
			no_trees[2] = x2;
			no_trees[3] = z2;
		}
		return k;
	}

	public void OnTreeFall( int x, int z, int facing_x, int facing_z ) {
		int x2 = x + facing_x;
		int z2 = z + facing_z;
		if ( !IsIndexValid(x2,z2) ) return;
		StartCoroutine(_OnTreeFallDomino(x2,z2,facing_x,facing_z));
	}
	IEnumerator _OnTreeFallDomino (int x, int z, int f_x, int f_z ) {
		yield return new WaitForSeconds(domino_fall_delay);
		if ( tree_list[x,z].isActiveAndEnabled) {
			tree_list[x,z].Fall(f_x,f_z);
		} 
		//tree_list[x-f_x,z-f_z].ForceDieNoDelay();
	}

	void Update () {
		foreach ( v2Tree tree in tree_list ) {
			if ( tree.isActiveAndEnabled ) tree.Grow();
		}
	}

	#if UNITY_EDITOR
	void OnDrawGizmos () {
		for(int i=0; i < total_tree_z; i++ ) {
			for(int j=0; j < total_tree_x; j ++ ) {
				Gizmos.DrawSphere(new Vector3(j*x_grid_offset+grid_root.x, 0, i*z_grid_offset+grid_root.z ),0.05f);
			}
		}
	}
	#endif
}	


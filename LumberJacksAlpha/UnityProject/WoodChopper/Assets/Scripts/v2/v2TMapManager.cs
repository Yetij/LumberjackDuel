using UnityEngine;
using System.Collections;

public class v2TMapManager : MonoBehaviour
{
	#region hiden
	private static v2TMapManager _instance;
	public static v2TMapManager Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(v2TMapManager)) as v2TMapManager;
				if ( _instance == null ) throw new UnityException("Object of type v2TreeManager not found");
			}
			return _instance;
		}
	}
	#endregion
	
	public Vector3 grid_root;
	public float x_grid_offset;
	public float z_grid_offset;
	public int total_tree_x;
	public int total_tree_z;

	Tree[,] tree_list;

#if UNITY_EDITOR
	void OnDrawGizmos () {
		for(int i=0; i < total_tree_x; i++ ) {
			for(int j=0; j < total_tree_z; j ++ ) {
				Gizmos.DrawSphere(new Vector3(j*x_grid_offset+grid_root.x, 0, i*z_grid_offset+grid_root.z ),0.05f);
			}
		}
	}
#endif

	void Awake () {
		tree_list = new Tree[total_tree_x,total_tree_z];
	}

	/* x=-1, z=-1 if object is not located on map  */
	public void GetCellIndex (Vector3 pos, out int x, out int z ) { 
		x = (int)Mathf.Floor((pos.x - grid_root.x )/x_grid_offset);
		z = (int)Mathf.Floor((pos.z - grid_root.z )/z_grid_offset);
		if ( z < 0 | z >= total_tree_z | x < 0 | x >= total_tree_z) { 
			x = -1;
			z = -1;
		}
	}

	Vector3 _tmp = Vector3.zero;
	public Vector3 MoveTo (int x, int z ) {
		_tmp.Set(grid_root.x+x*x_grid_offset + x_grid_offset/2, 0, grid_root.z + z*z_grid_offset + z_grid_offset/2 );
		return _tmp;
	}
	public void PlanRandomTree () {
	}
}


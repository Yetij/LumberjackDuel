using UnityEngine;
using System.Collections.Generic;

public class v4Pool : MonoBehaviour 
{
	List<v4Tree> pool;

	public void Initialize (int init_cap) {
		pool = new List<v4Tree> (init_cap);
		for(int i=0;i < init_cap; i++ ) {
		//	pool[i] = 
		}
	}

	public v4Tree Get () { return null;}
	public void Return(v4Tree t ) {}
}


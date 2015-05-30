using UnityEngine;
using System.Collections;

public class TreeAnimation : MonoBehaviour {

	public Tree parentTree;

	void Start ()
	{
		parentTree = gameObject.transform.GetComponentInParent<Tree> ();
	}
	
	void Death()
	{
		parentTree.Death ();
	}
}
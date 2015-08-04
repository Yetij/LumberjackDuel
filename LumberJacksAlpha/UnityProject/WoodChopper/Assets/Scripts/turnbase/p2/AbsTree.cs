using UnityEngine;
using System.Collections;

public class AbsTree : MonoBehaviour
{
	[HideInInspector] public p2Cell cell;

	virtual public bool IsPassable () {
		return false;
	}
}


using UnityEngine;
using System.Collections;

public class v4Tree : MonoBehaviour
{
	float STOP_CUT_WAIT_TIME = 0.15f;
	IEnumerator StopCut;

	public bool needCutAnimation;
	public bool trigger2;

	void OnBeingCut () {}

	void OnStopBeingCut () {}

}


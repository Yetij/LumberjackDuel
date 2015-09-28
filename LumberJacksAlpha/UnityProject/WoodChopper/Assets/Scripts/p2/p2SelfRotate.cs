using UnityEngine;
using System.Collections;

public class p2SelfRotate : MonoBehaviour {
	public Vector3 rotationAmount;
	// Update is called once per frame
	void Update () {
		transform.Rotate(rotationAmount*Time.deltaTime);
	}
}

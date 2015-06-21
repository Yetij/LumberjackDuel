using UnityEngine;
using System.Collections;

public class SkyboxRotate : MonoBehaviour {

	public float rotSpeed = 5;

	Vector3 z = Vector3.zero;

	void Update () {
		transform.RotateAround(z,Vector3.up, rotSpeed*Time.deltaTime);
	}
}

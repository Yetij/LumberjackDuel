using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class p1ConfirmButton : MonoBehaviour
{
	public Button OkButton;
	public Button cancelButton;

	public bool noSignal;
	public bool isOk;

	void OnEnable () {
		noSignal = true;
	}

	public void ConfirmOk () {
		noSignal = false;
		isOk = true;
	}

	public void ConfirmCancel () {
		noSignal = false;
		isOk = false;
	}
}


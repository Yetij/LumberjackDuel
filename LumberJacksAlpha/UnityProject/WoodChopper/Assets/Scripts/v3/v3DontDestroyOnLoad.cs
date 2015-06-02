using UnityEngine;
using System.Collections;

public class v3DontDestroyOnLoad : MonoBehaviour
{

	// Use this for initialization
	void Awake ()
	{
		Debug.Log(Application.loadedLevelName+": Awake");
		DontDestroyOnLoad(gameObject);
	}
}


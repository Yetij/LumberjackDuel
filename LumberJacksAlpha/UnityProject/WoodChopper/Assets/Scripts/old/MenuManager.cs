using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour {

	public void BeginDuel()
	{
		Application.LoadLevel (1);
	}

	public void ApplicationQuit()
	{
		Application.Quit ();
	}
}

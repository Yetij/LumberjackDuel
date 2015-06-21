using UnityEngine;
using System.Collections;

public class TestWWW : MonoBehaviour {

	void Start () {
		string url = "http://example.com/script.php?var1=value2&amp;var2=value2";
		WWW www = new WWW(url);
	}
	
	IEnumerator WaitForRequest(WWW www)
	{
		Debug.Log("haha12 !");
		yield return www;
		Debug.Log("haha !");
		
		// check for errors
		if (www.error == null)
		{
			Debug.Log("WWW Ok!: " + www.data);
		} else {
			Debug.Log("WWW Error: "+ www.error);
		}    
	}
}

using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

public class SimpleAds : MonoBehaviour 
{
	void Start ()  {
		Advertisement.Initialize ("43558", true);
		
		StartCoroutine (ShowAdWhenReady());
	}
	
	IEnumerator ShowAdWhenReady()
	{
		while (!Advertisement.isReady())
			yield return null;
		
		Advertisement.Show ();
	}
}


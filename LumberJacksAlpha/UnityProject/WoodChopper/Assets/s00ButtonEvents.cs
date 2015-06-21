using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

public class s00ButtonEvents : MonoBehaviour {
	public string androidGameID = "24299";

	void Start () {
		Debug.Log("Start");
		Advertisement.Initialize(androidGameID,true);
		StartCoroutine(LogWhenUnityAdsIsInitialized());
	}
	
	string zoneID = "ads00";
	public void Play () {
		Debug.Log("Play is pressed");
		ShowAd(zoneID);
	}

	public void Quit () {
	}
	private IEnumerator LogWhenUnityAdsIsInitialized ()
	{
		float initStartTime = Time.time;
		Debug.Log("LogWhenUnityAdsIsInitialized...");
		do{
			Debug.Log("loading ads");
			yield return new WaitForSeconds(0.1f);
		}
		while (!Advertisement.isInitialized);
		
		Debug.Log(string.Format("Unity Ads was initialized in {0:F1} seconds.",Time.time - initStartTime));
		yield break;
	}
	private static void HandleShowResult (ShowResult result)
	{
		switch (result)
		{
		case ShowResult.Finished:
			Debug.Log("The ad was successfully shown.");
			break;
		case ShowResult.Skipped:
			Debug.LogWarning("The ad was skipped before reaching the end.");
			break;
		case ShowResult.Failed:
			Debug.LogError("The ad failed to be shown.");
			break;
		}
		
		_onContinue();
	}

	static void _onContinue () {
		Debug.Log("Continue here ");
	}
	
	public static void ShowAd (string zoneID) 
	{
		if (string.IsNullOrEmpty(zoneID)) zoneID = null;
		
		if (Advertisement.isReady(zoneID))
		{
			Debug.Log("Showing ad now...");
			
			ShowOptions options = new ShowOptions();
			options.resultCallback = HandleShowResult;
			options.pause = true;
			
			Advertisement.Show(zoneID,options);
		}
		else 
		{
			Debug.Log(string.Format("Unable to show ad. The ad placement zone {0} is not ready.",
			                               object.ReferenceEquals(zoneID,null) ? "default" : zoneID));
		}
	}

}
